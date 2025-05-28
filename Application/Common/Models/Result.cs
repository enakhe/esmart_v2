#nullable disable

using ESMART.Domain.Entities.Data;

namespace ESMART.Application.Common.Models
{
    public class Result
    {
        internal Result(bool succeeded, IEnumerable<string> errors, ApplicationUser data)
        {
            Succeeded = succeeded;
            Errors = [.. errors];
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public ApplicationUser Response { get; set; }

        public static Result Success(ApplicationUser data)
        {
            return new Result(true, [], data);
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, errors, null);
        }
    }

    public class GuestServiceResult
    {
        internal GuestServiceResult(bool succeeded, IEnumerable<string> errors, string data)
        {
            Succeeded = succeeded;
            Errors = [.. errors];
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public string Response { get; set; }

        public static GuestServiceResult Success(string data)
        {
            return new GuestServiceResult(true, [], data);
        }

        public static GuestServiceResult Failure(IEnumerable<string> errors)
        {
            return new GuestServiceResult(false, errors, null);
        }
    }
}
