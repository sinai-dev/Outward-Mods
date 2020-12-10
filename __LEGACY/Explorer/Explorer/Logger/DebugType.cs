namespace Explorer
{

    //store message and message Color data
    internal struct DebugType
    {

        public string message;
        public string messageColor;

        public DebugType(string _msg, string _msgColor)
        {
            message = _msg;
            messageColor = _msgColor;
        }

    }

}