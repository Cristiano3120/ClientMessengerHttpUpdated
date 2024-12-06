using ClientMessengerHttpUpdated.LocalDatabaseClasses;
using System.Text.Json;

namespace ClientMessengerHttpUpdated
{
    internal static class JsonExtensions
    {
        internal static OpCode GetOpCode(this JsonElement property)
        {
            if (property.ValueKind != JsonValueKind.Number)
            {
                throw new InvalidOperationException("Cannot convert a non-numeric JSON property to an enum.");
            }

            byte value;
            try
            {
                value = property.GetByte();
            }
            catch (FormatException)
            {
                throw new Exception("The JSON number is not a valid integer.");
            }
            catch (OverflowException)
            {
                throw new Exception("The JSON number is outside the valid range for an OpCodeEnum value.");
            }

            return Enum.IsDefined(typeof(OpCode), value)
                ? (OpCode)value
                : throw new Exception("The JSON number does not correspond to any defined OpCodeEnum value.");
        }

        internal static UnexpectedError GetUnexpectedError(this JsonElement property)
        {
            if (property.ValueKind != JsonValueKind.Number)
            {
                throw new InvalidOperationException("Cannot convert a non-numeric JSON property to an enum.");
            }

            byte value;
            try
            {
                value = property.GetByte();
            }
            catch (FormatException)
            {
                throw new Exception("The JSON number is not a valid integer.");
            }
            catch (OverflowException)
            {
                throw new Exception("The JSON number is outside the valid range for an UnexpectedError value.");
            }

            return Enum.IsDefined(typeof(UnexpectedError), value)
                ? (UnexpectedError)value
                : throw new Exception("The JSON number does not correspond to any defined UnexpectedError value.");
        }


        internal static User? GetUser(this JsonElement property)
        {
            try
            {
                return new User()
                {
                    ProfilPic = Client.ByteArrayToBitmapImage(property.GetProperty("profilPic").GetBytesFromBase64()),
                    Id = property.GetProperty("id").GetInt64(),
                    Username = property.GetProperty("username").GetString()!,
                };
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return null;
            }
        }
    }
}

