namespace ClientMessengerHttpUpdated
{
    internal enum OpCode : byte
    {
        UnexpectedError = 0,
        ReceiveRSA = 1,
        SendAes = 2,
        ServerReadyToReceive = 3,
        TryToCreateAnAcc = 4,
        ResponseRequestToCreateAcc = 5,
        VerificationProcess = 6,
        VerificationResult = 7,
        RequestToLogin = 8,
        ResponseToLogin = 9,
    }
}
