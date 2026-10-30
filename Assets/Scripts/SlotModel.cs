using System;
namespace SlotGame {
 public interface IRandomSource { int Next(int count); }
 public sealed class RandomSource : IRandomSource { readonly Random random=new Random(); public int Next(int count)=>random.Next(count); }
 public sealed class OutcomeGenerator { readonly IRandomSource random; public OutcomeGenerator(IRandomSource random){this.random=random;} public int[] Spin()=>new[]{random.Next(4),random.Next(4),random.Next(4)}; }
 public static class PayoutEvaluator { public static int Evaluate(int[] result,int bet,int[] multipliers)=>result[0]==result[1]&&result[1]==result[2]?checked(bet*multipliers[result[0]]):0; }
 public sealed class Wallet { public int Credits {get;private set;} public Wallet(int credits){Credits=Math.Max(0,credits);} public bool Spend(int amount){if(amount<=0||Credits<amount)return false;Credits-=amount;return true;} public void Award(int amount){Credits=checked(Credits+Math.Max(0,amount));} }
}
