using ReCalc1Ex1.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ReCalc1Ex1
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString_Current = @"server=localhost;database=TPPXCurrent_ReCalc1;integrated Security=SSPI;";
            string connectionString_Audit = @"server=localhost;database=TPPXAudit_Recalc1;integrated Security=SSPI;";

            var Assets = GetAssets(connectionString_Current);
            var AssetPercentGoods = GetAssetPercentGoods(connectionString_Current);
            var AssetFactors = GetAssetFactors(connectionString_Current);

            FillCalculatedAssetValues(Assets, AssetPercentGoods, AssetFactors);

            Console.WriteLine("Bye ...");

        }

        private static List<Asset> GetAssets(string connectionString_Current)
        {
            var Assests = new List<Asset>();

            using (SqlConnection con = new SqlConnection(connectionString_Current))
            {
                string stm1 = @"SELECT Asset.Asset.HistoryRunId, Asset.Asset.Id, Asset.Asset.Folio, Asset.Asset.AcquisitionYear, Asset.Asset.CostValue, Asset.Asset.[Index], Asset.Asset.Life, Asset.Asset.Depreciation, Asset.Asset.AdjustmentFactor, 
                            Asset.Asset.CalculatedValue, Asset.Asset.IsResidual, Asset.Asset.Schedule, Asset.Asset.RollYear
                        FROM            Asset.Asset INNER JOIN
                                Account.CompanyFolio ON Asset.Asset.HistoryRunId = Account.CompanyFolio.HistoryRunId AND Asset.Asset.Folio = Account.CompanyFolio.Folio
                                WHERE Status_Code = 'AC'";
                using (SqlCommand cmd = new SqlCommand(stm1, con))
                {
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Assests.Add(new Asset
                        {
                            AcquisitionYear = (Int16)reader["AcquisitionYear"],
                            AdjustmentFactor = (decimal)reader["AdjustmentFactor"],
                            CalculatedValue = (decimal)reader["CalculatedValue"],
                            CostValue = (decimal)reader["CostValue"],
                            Depreciation = (decimal)reader["Depreciation"],
                            Folio = (string)reader["Folio"],
                            HistoryRunId = (int)reader["HistoryRunId"],
                            Id = (int)reader["Id"],
                            Index = (string)reader["Index"],
                            IsResidual = (bool)reader["IsResidual"],
                            Life = (int)reader["Life"],
                            Schedule = (string)reader["Schedule"],
                            RollYear = (int)reader["RollYear"]
                        });
                    }
                    con.Close();
                }
            }

            return Assests;
        }


        private static List<AssetPercentGood> GetAssetPercentGoods(string connectionString_Current)
        {
            var AssetsPercentGoods = new List<AssetPercentGood>();

            using (SqlConnection con = new SqlConnection(connectionString_Current))
            {
                string stm1 = @"SELECT HistoryRunId,Life,EffectiveAge,Rate FROM Asset.AssetPercentGood Order By Life,HistoryRunId";
                using (SqlCommand cmd = new SqlCommand(stm1, con))
                {
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        AssetsPercentGoods.Add(new AssetPercentGood
                        {
                            HistoryRunId = (int)reader["HistoryRunId"],
                            Life = (int)reader["Life"],
                            EffectiveAge = (int)reader["EffectiveAge"],
                            Rate = (decimal)reader["Rate"]
                        });
                    }
                    con.Close();
                }
            }

            return AssetsPercentGoods;
        }

        private static List<AssetFactor> GetAssetFactors(string connectionString_Current)
        {
            var AssetsFactor = new List<AssetFactor>();

            using (SqlConnection con = new SqlConnection(connectionString_Current))
            {
                string stm1 = @"SELECT HistoryRunId,[Index],AcqYear,Factor FROM Asset.AssetFactor";
                using (SqlCommand cmd = new SqlCommand(stm1, con))
                {
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        AssetsFactor.Add(new AssetFactor
                        {
                            HistoryRunId = (int)reader["HistoryRunId"],
                            Index = (string)reader["Index"],
                            AcqYear = (int)reader["AcqYear"],
                            Factor = (decimal)reader["Factor"]
                        });
                    }
                    con.Close();
                }
            }

            return AssetsFactor;
        }


        private static void FillCalculatedAssetValues(List<Asset> assets, List<AssetPercentGood> assetPercentGoods, List<AssetFactor> assetFactors)
        {
            foreach (var asset in assets)
            {

                var item = assetPercentGoods.Find(a => a.Life == asset.Life && a.HistoryRunId == asset.HistoryRunId);
                var effectiveAge = item.EffectiveAge;
                var rate = item.Rate;

                if (assetPercentGoods.Exists(a => a.HistoryRunId == asset.HistoryRunId && a.Life == asset.Life)) 
                    asset.ResidualFlag = (asset.IsResidual == true) || (asset.AcquisitionYear == effectiveAge);
                else
                    asset.ResidualFlag = true;

                if (asset.ResidualFlag)
                    asset.EffectiveAge = effectiveAge;
                else
                    asset.EffectiveAge = asset.AcquisitionYear;

                if (assetPercentGoods.Exists(a => a.HistoryRunId == asset.HistoryRunId && a.Life == asset.Life))
                    asset.Depreciation2 = 100 - rate;
                else
                {
                    asset.EffectiveAge = effectiveAge;
                    asset.ResidualFlag = true;
                    asset.Depreciation2 = 100 - rate;
                }

                asset.Factor = assetFactors.Find( a =>  a.HistoryRunId == asset.HistoryRunId && a.AcqYear == asset.EffectiveAge && a.Index == asset.Index).Factor;

                asset.ReplacementCostNew = Math.Truncate(asset.CostValue * asset.Factor);

                asset.CalculatedValueNew = Math.Round((asset.CostValue * asset.Factor) * (100 - asset.Depreciation2) / 100 * (100 - asset.AdjustmentFactor) / 100, MidpointRounding.AwayFromZero);

            }

        }
    }


}
