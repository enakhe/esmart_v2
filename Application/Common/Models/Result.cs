using ESMART.Domain.Entities.Data;

namespace ESMART.Application.Common.Models
{
    public class Result
    {
        internal Result(bool succeeded, IEnumerable<string> errors, ApplicationUser data)
        {
            Succeeded = succeeded;
            Errors = errors.ToArray();
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public ApplicationUser Response { get; set; }

        public static Result Success(ApplicationUser data)
        {
            return new Result(true, Array.Empty<string>(), data);
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, errors, null);
        }
    }
}
