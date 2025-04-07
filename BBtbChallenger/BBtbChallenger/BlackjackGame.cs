using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBtbChallenger
{
    public class BlackjackGame
    {
        private static readonly Random rng = new Random();
        public List<string> PlayerHand { get; private set; } = new();
        public List<string> DealerHand { get; private set; } = new();
        public bool IsGameOver { get; private set; } = false;

        public BlackjackGame()
        {
            PlayerHand.Add(DrawCard());
            PlayerHand.Add(DrawCard());
            DealerHand.Add(DrawCard());
        }

        private string DrawCard()
        {
            string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
            return ranks[rng.Next(ranks.Length)];
        }

        private int CalculateHand(List<string> hand)
        {
            int total = 0, aces = 0;
            foreach (var card in hand)
            {
                if (int.TryParse(card, out int val))
                    total += val;
                else if (card == "A")
                {
                    aces++;
                    total += 11;
                }
                else
                    total += 10;
            }

            while (total > 21 && aces > 0)
            {
                total -= 10;
                aces--;
            }

            return total;
        }

        public string GetPlayerHand() => string.Join(", ", PlayerHand) + $" (Total: {CalculateHand(PlayerHand)})";
        public string GetDealerHand(bool reveal = false)
        {
            return reveal ? string.Join(", ", DealerHand) + $" (Total: {CalculateHand(DealerHand)})"
                          : DealerHand.First() + ", ❓";
        }

        public bool PlayerHit()
        {
            PlayerHand.Add(DrawCard());
            if (CalculateHand(PlayerHand) > 21)
            {
                IsGameOver = true;
                return false; // bust
            }
            return true;
        }

        public string PlayerStand()
        {
            while (CalculateHand(DealerHand) < 17)
            {
                DealerHand.Add(DrawCard());
            }

            IsGameOver = true;
            int playerTotal = CalculateHand(PlayerHand);
            int dealerTotal = CalculateHand(DealerHand);

            if (dealerTotal > 21 || playerTotal > dealerTotal)
                return "You win!";
            else if (playerTotal == dealerTotal)
                return "It's a tie!";
            else
                return "Dealer wins!";
        }
    }

}
