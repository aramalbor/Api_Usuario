using Api_Usuario.Data;
using Api_Usuario.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Api_Usuario.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly UsuarioContext _context;


        private HttpClient _httpClient;

        public StorageController(UsuarioContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://talkday1.igrtec.cloud/index.php/wp-json/wp/v2/posts");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOjEsIm5hbWUiOiJjbG91ZF8xIiwiaWF0IjoxNjg2OTcyMDc1LCJleHAiOjE4NDQ2NTIwNzV9.q0tSiFwle3vAd16O2pJDErL0oSTyO2PPyMSRgBdaxpU"); // Reemplaza con tus credenciales codificadas en Base64
        }

        [HttpPost]
        [Route("UploadStorage")]
        public async Task<ActionResult<string>> UploadAndReturnImageUrl(IFormFile file , string titulo , string subtitulo , int repeticiones, string uidUser )
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No se ha proporcionado ningún archivo.");
            }

            try
            {
                // Leer los datos del archivo
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                // Crear la solicitud HTTP
                using (var content = new MultipartFormDataContent())
                {
                    var imageContent = new ByteArrayContent(fileBytes);
                   // imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                    content.Add(imageContent, "file", file.FileName);

                    // Enviar la solicitud a la API de WordPress
                    var response = await _httpClient.PostAsync("media", content);

                    // Verificar si la respuesta fue exitosa
                    if (response.IsSuccessStatusCode)
                    {
                        // Leer la respuesta como JSON
                        var json = await response.Content.ReadAsStringAsync();

                        // Analizar el JSON para obtener la URL de la imagen
                        dynamic data = JsonConvert.DeserializeObject(json);
                        string imageUrl = data?.source_url;

                        var dataStorage = new Storage()
                        {   
                            Titulo = titulo,
                            Subtitulo = subtitulo,
                            Repeticiones = repeticiones,
                            Fecha= DateTime.Now,
                            UrlArchivo=imageUrl,
                            UidUser= uidUser

                        };
                        _context.Storage.Add(dataStorage);
                        _context.SaveChanges();
                        return Ok(dataStorage);
                    }
                    else
                    {
                        // Si la respuesta no fue exitosa, lanzar una excepción o manejar el error según sea necesario
                        return StatusCode((int)response.StatusCode, $"Error al subir la imagen: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que ocurra durante el proceso
                return StatusCode(500, $"Error al subir la imagen: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetStorageUsage")]
        public async Task<ActionResult<string>> GetStorageUsage()
        {
            try
            {
                // Hacer una solicitud GET a la API de WordPress para obtener la lista de imágenes
                var response = await _httpClient.GetAsync("media");

                // Verificar si la respuesta fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta como JSON
                    var json = await response.Content.ReadAsStringAsync();

                    // Analizar el JSON para obtener la lista de imágenes
                    dynamic data = JsonConvert.DeserializeObject(json);
                    int totalImages = data?.Count ?? 0;
                    long totalStorageUsage = 0;

                    // Obtener el tamaño de cada imagen individualmente
                    foreach (var image in data)
                    {
                        // Obtener el enlace de la imagen
                        string imageUrl = image?.source_url;

                        // Hacer una solicitud HEAD para obtener los encabezados de la imagen
                        var imageResponse = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, imageUrl));

                        // Verificar si la respuesta fue exitosa
                        if (imageResponse.IsSuccessStatusCode)
                        {
                            // Obtener el tamaño de la imagen desde el encabezado 'Content-Length'
                            if (imageResponse.Content.Headers.TryGetValues("Content-Length", out var values))
                            {
                                if (long.TryParse(values.FirstOrDefault(), out long imageSize))
                                {
                                    totalStorageUsage += imageSize;
                                }
                            }
                        }
                    }

                    // Convertir el espacio utilizado a megabytes con dos decimales sin redondear
                    double totalStorageUsageMB = (double)totalStorageUsage / (1024 * 1024);

                    // Calcular el espacio restante en megabytes
                    const long totalStorageLimit = 100 * 1024 * 1024; // Limite total de almacenamiento en bytes
                    double remainingStorageMB = ((double)totalStorageLimit - totalStorageUsage) / (1024 * 1024);

                    return Ok($"Total de imágenes: {totalImages}, Almacenamiento utilizado: {totalStorageUsageMB:F2} MB, Espacio restante: {remainingStorageMB:F2} MB");
                }
                else
                {
                    // Si la respuesta no fue exitosa, lanzar una excepción o manejar el error según sea necesario
                    return StatusCode((int)response.StatusCode, $"Error al obtener la información de almacenamiento: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que ocurra durante el proceso
                return StatusCode(500, $"Error al obtener la información de almacenamiento: {ex.Message}");
            }
        }





        private int GetTotalStorageUsageFromContentRange(string contentRange)
        {
            // Implementa el código para analizar el valor de Content-Range y extraer el tamaño total de almacenamiento utilizado en bytes
            // Dependiendo del formato de Content-Range, puede ser necesario realizar algún procesamiento adicional para obtener el valor correcto
            // Aquí tienes un ejemplo de implementación básica que extrae el tamaño total en bytes:
            int totalStorageUsage = 0;
            if (!string.IsNullOrEmpty(contentRange))
            {
                var rangeValues = contentRange.Split('/');
                if (rangeValues.Length == 2 && int.TryParse(rangeValues[1], out totalStorageUsage))
                {
                    return totalStorageUsage;
                }
            }
            return totalStorageUsage;
        }

    }
}
