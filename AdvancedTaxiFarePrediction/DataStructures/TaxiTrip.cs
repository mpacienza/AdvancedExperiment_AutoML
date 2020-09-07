using Microsoft.ML.Data;

namespace AdvancedTaxiFarePrediction.DataStructures
{
    public class TaxiTrip
    {
        [ColumnName("vendor_id")]
        public string VendorId;

        [ColumnName("rate_code")]
        public float RateCode;

        [ColumnName("passenger_count")]
        public float PassengerCount;

        [ColumnName("trip_time_in_secs")]
        public float TripTime;

        [ColumnName("trip_distance")]
        public float TripDistance;

        [ColumnName("payment_type")]
        public string PaymentType;

        [ColumnName("fare_amount")]
        public float FareAmount;
    }

    public class AXabc
    {


        [LoadColumn(0)]
        public float oscporc { get; set; }
        [LoadColumn(1)]
        public float xab { get; set; }
        [LoadColumn(2)]
        public float abc { get; set; }
        [LoadColumn(3)]
        public float bcd { get; set; }
        [LoadColumn(4)]
        public float maximo { get; set; }
        [LoadColumn(5)]
        public float operacion { get; set; }
        [LoadColumn(6)]
        public float zonaposicion { get; set; }
        [LoadColumn(7)]
        public float enzona { get; set; }
        [LoadColumn(8)]
        public float retracement { get; set; }
        [LoadColumn(9)]
        public float oscvalor { get; set; }
        [LoadColumn(10)]
        public float oscvalor70 { get; set; }
        [LoadColumn(11)]
        public float oscstatus { get; set; }
        [LoadColumn(12)]
        public float wxa { get; set; }
        [LoadColumn(13)]
        public float wbc { get; set; }
        [LoadColumn(14)]
        public float max1000 { get; set; }
        [LoadColumn(15)]
        public float vh1 { get; set; }
        [LoadColumn(16)]
        public float vh2 { get; set; }
        [LoadColumn(17)]
        public float vol1 { get; set; }
        [LoadColumn(18)]
        public float vol2 { get; set; }
        [LoadColumn(19)]
        public float ccia { get; set; }
        [LoadColumn(20)]
        public float ccic { get; set; }
        [LoadColumn(21)]
        public float time { get; set; }
        [LoadColumn(22)]
        public float osca { get; set; }
        [LoadColumn(23)]
        public float oscb { get; set; }
        [LoadColumn(24)]
        public float oscw { get; set; }


        [LoadColumn(25)]
        public float abc2 { get; set; }
        [LoadColumn(26)]
        public float retracement2 { get; set; }
        [LoadColumn(27)]
        public float atrf { get; set; }
        [LoadColumn(28)]
        public float atrl { get; set; }
        [LoadColumn(29)]
        public float emaf { get; set; }
        [LoadColumn(30)]
        public float emam { get; set; }
        [LoadColumn(31)]
        public float emal { get; set; }

        [LoadColumn(32)]
        public float dm { get; set; }
        [LoadColumn(33)]
        public float dip { get; set; }
        [LoadColumn(34)]
        public float din { get; set; }
        [LoadColumn(35)]
        public float sma6 { get; set; }
        [LoadColumn(36)]
        public float sma4 { get; set; }
        [LoadColumn(37)]
        public float mfia { get; set; }
        [LoadColumn(38)]
        public float mfib { get; set; }
        [LoadColumn(39)]
        public float mfic { get; set; }
        [LoadColumn(40)]
        public float vwap { get; set; }
    }
}