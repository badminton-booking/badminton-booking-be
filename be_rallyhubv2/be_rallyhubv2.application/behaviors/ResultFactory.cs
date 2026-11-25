using System.Reflection;
using be_rallyhubv2.contract.abstractions.results;

namespace be_rallyhubv2.application.behaviors;

internal static class ResultFactory
{
     public static TResponse Failure<TResponse>(params Error[] errors) where TResponse : Result
     {
          if(typeof(TResponse) == typeof(Result))
               return (TResponse)Result.Failure(errors);

          var failure = typeof(TResponse).GetMethod(
               nameof(Result.Failure),
               BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
               new[] { typeof(Error[]) })!;
          
          return (TResponse)failure.Invoke(null, new object[] { errors })!;
     }
     //Result<T>.Failure không được, c# không cho vì ko xác định đưc T là gì
     //trick
}    