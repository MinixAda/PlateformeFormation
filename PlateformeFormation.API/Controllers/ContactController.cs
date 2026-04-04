
// Controllers/ContactController.cs
//   1. Installer le package Resend 0.2.2 via NuGet
//   2. Ajouter dans appsettings.json :
//        "Resend": { "ApiKey": "" },
//        "Contact": { "Destinataire": "minix.ada@gmail.com" }
//   3. Stocker la clé API en User Secrets :
//        dotnet user-secrets set "Resend:ApiKey" "re_xxxxxxxxxxxx"
//   4. Ne pas oublier de répercuter dans Program.cs
//   les services liés à Resend (AddHttpClient, Configure, AddTransient)

using Microsoft.AspNetCore.Mvc;
using Resend;

namespace PlateformeFormation.API.Controllers;

// Dto reçu depuis React
public record ContactDto(
    string Objet,
    string Message,
    string Email,
    bool EnvoyerCopie
);

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IResend _resend;
    private readonly IConfiguration _config;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IResend resend, IConfiguration config, ILogger<ContactController> logger)
    {
        _resend = resend;
        _config = config;
        _logger = logger;
    }

    // POST /api/contact 
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ContactDto dto)
    {
        // 1. Validation 
        if (string.IsNullOrWhiteSpace(dto.Objet) || dto.Objet.Trim().Length < 3)
            return BadRequest(new { error = "L'objet est trop court." });

        if (string.IsNullOrWhiteSpace(dto.Message) || dto.Message.Trim().Length < 20)
            return BadRequest(new { error = "Le message est trop court." });

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            return BadRequest(new { error = "Adresse email invalide." });

        // 2. Adresse destinataire (masquée côté serveur) 
        var destinataire = _config["Contact:Destinataire"];
        if (string.IsNullOrEmpty(destinataire))
        {
            _logger.LogError("Contact:Destinataire manquant dans appsettings.");
            return StatusCode(500, new { error = "Configuration serveur manquante." });
        }

        try
        {
            // 3. Email principal --> destinataire masqué
            var emailPrincipal = new EmailMessage
            {
                From = "FormaPro <onboarding@resend.dev>",
                Subject = $"[FormaPro Contact] {dto.Objet.Trim()}",
                HtmlBody = BuildEmailPrincipal(dto),
                ReplyTo = dto.Email.Trim(),
            };
            emailPrincipal.To.Add(destinataire);

            await _resend.EmailSendAsync(emailPrincipal);

            // 4. Copie optionnelle --> vers l'expéditeur
            if (dto.EnvoyerCopie)
            {
                var emailCopie = new EmailMessage
                {
                    From = "FormaPro <onboarding@resend.dev>",
                    Subject = $"Copie de votre message : {dto.Objet.Trim()}",
                    HtmlBody = BuildEmailCopie(dto),
                };
                emailCopie.To.Add(dto.Email.Trim());

                await _resend.EmailSendAsync(emailCopie);
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi du formulaire de contact.");
            return StatusCode(500, new { error = "L'envoi a échoué. Veuillez réessayer." });
        }
    }

    // 5. Templates HTML
    private static string BuildEmailPrincipal(ContactDto dto) => $@"
        <div style='font-family:sans-serif;max-width:600px;margin:0 auto'>
          <h2 style='color:#1565C0;border-bottom:2px solid #E3F2FD;padding-bottom:8px'>
            Nouveau message de contact — FormaPro
          </h2>
          <table style='width:100%;border-collapse:collapse'>
            <tr>
              <td style='padding:8px 0;color:#6C757D;font-weight:600;width:140px'>Objet</td>
              <td style='padding:8px 0'>{Encode(dto.Objet)}</td>
            </tr>
            <tr>
              <td style='padding:8px 0;color:#6C757D;font-weight:600'>Expéditeur</td>
              <td style='padding:8px 0'>{Encode(dto.Email)}</td>
            </tr>
          </table>
          <div style='margin-top:16px;padding:16px;background:#F8F9FA;border-radius:8px;white-space:pre-wrap'>
            {Encode(dto.Message)}
          </div>
          <p style='margin-top:16px;font-size:12px;color:#6C757D'>
            Répondez directement à cet email pour contacter l'expéditeur.
          </p>
        </div>";

    private static string BuildEmailCopie(ContactDto dto) => $@"
        <div style='font-family:sans-serif;max-width:600px;margin:0 auto'>
          <h2 style='color:#1565C0'>Copie de votre message</h2>
          <p style='color:#6C757D'>
            Voici une copie du message que vous avez envoyé à l'équipe FormaPro.
          </p>
          <div style='margin-top:16px;padding:16px;background:#F8F9FA;border-radius:8px'>
            <strong>Objet :</strong> {Encode(dto.Objet)}<br/><br/>
            <div style='white-space:pre-wrap'>{Encode(dto.Message)}</div>
          </div>
          <p style='margin-top:16px;font-size:12px;color:#6C757D'>
            Ne répondez pas à cet email — c'est une copie automatique.
          </p>
        </div>";

    private static string Encode(string s) =>
        System.Net.WebUtility.HtmlEncode(s.Trim());
}