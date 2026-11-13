namespace CompeTournament.Shared.Tournaments
{
    using System;

    public class CreditTransactionDto
    {
        public int Id { get; set; }

        public int Amount { get; set; }

        public int BalanceAfter { get; set; }

        public string Reason { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
