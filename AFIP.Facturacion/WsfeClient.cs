using AFIP.Facturacion.Configuration;
using AfipServiceReference;
using Microsoft.Extensions.Options;
using PointOfSale.Model;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AFIP.Facturacion
{
    public class WsfeClient
    {
        private readonly IOptions<AFIPConfigurationOption> _configuration;
        private readonly LoginCmsClient _loginCmsClient;

        public string WsfeUrlHomologation { get; set; } = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx";
        public string WsfeUrlProd { get; set; } = "https://servicios1.afip.gov.ar/wsfev1/service.asmx";

        public WsfeClient(IOptions<AFIPConfigurationOption> configuration,
           LoginCmsClient loginCmsClient)
        {
            _configuration = configuration;
            _loginCmsClient = loginCmsClient;
            _loginCmsClient = loginCmsClient;
        }

        private async Task<FEAuthRequest> GetAuthRequestAsync(AjustesFacturacion ajustes)
        {
            var ticket = await _loginCmsClient.GetWsaaTicket(ajustes);
            return new FEAuthRequest { Cuit = ajustes.Cuit.Value, Sign = ticket.Sign, Token = ticket.Token };
        }

        private ServiceSoapClient GetServiceSoapClient()
        {
            var wsfeService = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            wsfeService.Endpoint.Address = new EndpointAddress(_configuration.Value.IsProdEnvironment ? WsfeUrlProd : WsfeUrlHomologation);
            return wsfeService;
        }

        public async Task<FECompUltimoAutorizadoResponse> FECompUltimoAutorizadoAsync(AjustesFacturacion ajustes, int ptoVta, int cbteTipo)
        {
            var wsfeService = GetServiceSoapClient();
            var auth = await GetAuthRequestAsync(ajustes);

            return await wsfeService.FECompUltimoAutorizadoAsync(auth, ptoVta, cbteTipo);
        }

        public async Task<FECAESolicitarResponse> FECAESolicitarAsync(AjustesFacturacion ajustes ,FECAERequest feCaeReq)
        {
            var wsfeService = GetServiceSoapClient();
            var auth = await GetAuthRequestAsync(ajustes);

            return await wsfeService.FECAESolicitarAsync(auth, feCaeReq);
        }
        public async Task<FECAEARegInformativoResponse> FECAEASolicitar_CAEA_Async(AjustesFacturacion ajustes , FECAEARequest feCaeReq)
        {
            var wsfeService = GetServiceSoapClient();
            var auth = await GetAuthRequestAsync(ajustes);

            var s2 = await wsfeService.FECAEASolicitarAsync(auth,202409,2);
            feCaeReq.FeDetReq[0].CAEA = s2.Body.FECAEASolicitarResult.ResultGet.CAEA;
            return await wsfeService.FECAEARegInformativoAsync(auth, feCaeReq);
        }
    }
}
