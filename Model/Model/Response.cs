using System;

namespace Domain.Model.Response
{
  public class Response<T>
    {
        public int? Status { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public Exception Error { get; set; }
    }

    public class Errors
    {
        public string InternalCode { get; set; }
        public string Message { get; set; }
    }
}