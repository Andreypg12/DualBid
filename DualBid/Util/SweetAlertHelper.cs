using System.Text.Json;

namespace Libreria.Web.Util
{
    public static class SweetAlertHelper
    {
        public static string CrearNotificacion(string titulo, string mensaje, SweetAlertMessageType tipo)
        {
            var config = new
            {
                title = titulo,
                text = mensaje,
                icon = tipo.ToString()
            };

            return JsonSerializer.Serialize(config);
        }
    }

    public enum SweetAlertMessageType
    {
        success,
        error,
        warning,
        info,
        question
    }
}
