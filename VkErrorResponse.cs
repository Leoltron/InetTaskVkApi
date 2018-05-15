namespace InetTaskVkApi
{
    class VkErrorResponse
    {
        public VkError error { get; set; }
    }

    class VkError
    {
        public int error_code { get; set; }
        public string error_msg { get; set; }
        public RequestParameter[] request_params { get; set; }
    }

    class RequestParameter
    {
        public string key { get; set; }
        public string value { get; set; }

    }
}