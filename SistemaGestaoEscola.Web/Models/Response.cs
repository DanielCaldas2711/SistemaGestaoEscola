﻿namespace SistemaGestaoEscola.Web.Models
{
    public class Response
    {
        public bool IsSuccess { get; set; }

        public string? Message { get; set; }


        public object? Results;
    }
}
