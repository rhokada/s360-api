using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Vision.v1;
using Google.Apis.Vision.v1.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PromoClicks.Common.Helper
{
    public static class GoogleVision
    {
        public static IList<AnnotateImageResponse> DetectText(String base64Image)
        {
            var service = CreateAuthorizedClient();

            // Convert image to Base64 encoded for JSON ASCII text based request   
            // Post text detection request to the Vision API
            var responses = service.Images.Annotate(
                new BatchAnnotateImagesRequest()
                {
                    Requests = new[] {
                    new AnnotateImageRequest() {
                        Features = new [] { new Feature() { Type =
                          "TEXT_DETECTION"}},
                        Image = new Image() { Content = base64Image }
                    }
               }
                }).Execute();
            return responses.Responses;
        }

        private static VisionService CreateAuthorizedClient()
        {
            var credenciais = new
            {
 /*               type = "service_account",
                project_id = "bdm---big-data-mining",
                private_key_id = "ef6b2217773d12b14af5f1b0cc7658700832874d",
                private_key = "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDEPyl44JK7HuGQ\nlDrFqhhqgPK5yFfTI311+1075SDp9EHDhXandUkyzx6ZRIXi54GdaXwY2TNGSbnK\nEnp1IFlDjL7nUFUNmYmfCnG8moIKv2jVlrHwOuz04hArhppcOrjKD6yvRz3DK68W\nf1nT1iJmm0MLgcIJHBaC83qK3zogIw00PrqzZEqjK7jcPUIdAdAWqTsCMbco/cYC\n4oT40B/XQ3VW8Aan1Ld/YYeMe7Wun2JQpf3vF+e+jmlOOr9lweYrk/zM+yxFN8nd\nRoLTJYeSHB2tqWtlR/A/iG8+tiNPp66xYZZfDfnpzu4V2svNR2extfYL3t8AF7QM\np6WHF9EpAgMBAAECggEBAImKaxqY6r3Syux1tytKJ4qdjYC/9S25tfi2j4Wm9QaD\ndoP+SkbbYM4Hc76f0+ZPsvwMLjmMIO+mRFxnzwPABE/fmPytxK6faWDaBRTWhUXb\nGTJ5R4TDB8gpmIOo9w6k6VZU3gaKu7LKLxBpeR+K+NlJABhUfRW9Zdmpo70o7qgO\nLsWYWSvRjsOQWV2n4qgg1GChnZQHxNJ3D7mcUSPOE2pIwB0b3x/e1Lz/TQlWzu13\navlKfOHygEz6I3ORk7ZKTCu/WO0TPoOwKApy5STgwpI/Y2BP4xLX4DYm7P+9CLG3\ngw4LK2f5VZngW3V3c5rSvy1nCjU5bTahdDH4Lq8PeMECgYEA4Z+IFygNuLG8j3M0\ntc35KfFhu2i/hgryXbLgksKfz57Sylm8e2MCShC4xr8o+7lZXLL+oKz042WCnOya\n3XtURsvZoeKqugAy41nqE9FNfV8S8OofAllQ3D7u/s5QRksT/hUX+ecTNxoB8UdA\nEnJ2Z36DEuiH/NaM9NBT4Z4ODT0CgYEA3qsfpk7Lk3PsyEPRaejGXdxjwTNxvcG0\nRV/WAxCsLqTao7BJqH9ay5/n8d3/8qwMKsDer3m9s4keylIf5cusf+2hfmeaxrl9\nT5d53R4fyNEtZK2sretgn1PTuFJhC6QnrAgWjN4l5AivBGYp9LXlWAluGhGNCzoo\nhucBsrQlKl0CgYAz2MkZKSceDhnajQrLzG05ajKfMk7wqotPZM+eagL3XvPpc/2Q\nDQx1oKoVYdA1KD6Uwr0ohxYvOyBslyJvSuvu6OE2BjRdUbZf8EWOEEbSTIq5udMu\nYv/l3MBm5VolDROnC4na73LG8r8Hhuv+UTdMSRItwimdvF/f0g+0hPGNsQKBgDc6\nXHREcOvWwfD+GS/ao3DeXAOsfdah99OJLKy/8uTy0KPu8qYPSHbe54mvFW+cSrxD\n8De3y1x/cxqdefnmJZfpWbfffJ0znqgiGwDEGwAVGdwx03Wjvuhlw0qhbB6WDZz2\nuJkh9GX13WsK5/chMow+coeWuNQDDdA/9cTcN6elAoGAPpYBocoouQM93vgLT1Z2\n36L8XqDHQz5MwX8f0EG9nXDmhQPIervRdlKrqggJ719vkOPtqWxPJtB4QHg2LMQZ\nbdrThXSr3hbvguQWPU1mxFkphyNNg/xuDAQ41k6xl1JDpyFb003NZyTajbwmW8Sg\n+LZDtIypDoBUW8+Y5QU/v7Y=\n-----END PRIVATE KEY-----\n",
                client_email = "965634327597-compute@developer.gserviceaccount.com",
                client_id = "110084474877080559240",
                auth_uri = "https://accounts.google.com/o/oauth2/auth",
                token_uri = "https://accounts.google.com/o/oauth2/token",
                auth_provider_x509_cert_url = "https://www.googleapis.com/oauth2/v1/certs",
                client_x509_cert_url = "https://www.googleapis.com/robot/v1/metadata/x509/965634327597-compute%40developer.gserviceaccount.com"
 */

                type = "service_account",
                project_id = "promoclicks-185313",
                private_key_id = "b8e5a927fad9eb3eb46e2212637cae373f2749b8",
                private_key = "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDZtSZiHrQ3ULMK\n7Ucb2qIQAaAuZNFCsHMD92GMFUv1CoXf2Q+pjLn/LlWSAQ9O3s7RnJ2SAPoV70uX\n0+0bOU59QmMiB4iYZvGjWxjI9oGQqjz3S7+Ej0uGpuwvrqOUEkGvcZy6XQGG3CoA\nj/vI5z4OWFk5CJNYyIAGGMCtT4Gj7yyJdMSuvnfPi5bxVU2aGNEcY/6AtCQkGkpN\nerrCzNP/M2PVYVZpGXHmVUsbwta1+hYNJ9KVZgcqiVzpIhoxCaD+t4LuRFrFqbcM\neB26LmSn7Qa1MsJ8bE9IvKSxXtw2YtpBKLKEUGJWh2iLJxvi99wh3PsdfL/ize0b\nHG99bJuBAgMBAAECggEAAKKU7q1/t7CpMDiD0sUtJtW3NPNwqs0lIFW5Q1KrlNs8\nAsKmgUMAvPf6UJk/PpuCY52Uhq4ARiR4DgtbLeD83+meDax9Nf6aZrtTZO8Ho9ml\nWlCI72nGqyuAb7FYX9Ugu4eJ/06paIWFVATVTR9Po4GtfUx9vBlsDFL81WLwlXQN\ndypJTfHARIXYUJ0pyHOyc6r+MGljsePRDI9dmnDSiPp4eKAQHxL/Dqwu/5wwswYa\n3DFH/aDCqRUoIx/KuM3nCiCY2PSXVBB5X1kuLd4WsAq1RhkM/QR4D/XGQTrESPx+\nAoDEnbljuW4Ovk8zfg39hGZrWqGEUU61UY+ZeVyEMQKBgQDv9meqL7zgETFsZQJe\n6V2gjk1InrykcbntEdCWY/LZ5n/5AOAuUYJPgv7JBfgoscyEgZB9UxXVPKTcAu1b\nbg241R15DXsrfHh1Xqg2p70XpDU8YGMr8W3fW/qbJTa4r+XM16ktlJu6rel2D8PC\ngyM9MeqrgbHnnPvwrhOMfb67uQKBgQDoQfqSFubluPXrpR0yaXEyoQBtPYFhs78L\neV/5zbAMfIth+ZIzBlwk5c2XtIz1OuQKszE6WIV8a/wDBa0bgwQ+GTYysd/hTyjM\nit5UasXLZxXZ5dX4u6BDL1cNmApzzmcIXMk/6q07bR0b7ZfEnf4I+PdWo/cChrqy\nFUxkcS0SCQKBgClljtizR5g4HxieDFynUbmjEv4WNGECJZsaWau4Lmsc8rLYTdRv\nx1VEOgQf/YG3upqZZ33XWYrh3Wb/Mkd1ovRh/6Wkh70myfkljUtwgJSVhYGW90Tb\nb6L60S72qowN/EzsX4k3e+4mloIBkjn6OZgTBnLz+ucEFAqfXUv9XWwxAoGAGz0R\nq+xvq+VjQf2dTPo8wllLeYRLFjBEaY4UiXFsAirhFd51HPLT/6fL0szj8yay8+ZM\nEbGsBgmMBra1tJJK/xVp28wsm3nzKPnoVTIofFBcqa9gxskKZJa6uOdp4mcEgmCP\nYaWeJ2gGj+3vbKmyz06Tg1+7alpurs/8o5L6XDECgYEAyptofVbMI/2ekdXE9k4j\nLEi+KNpR8kKWst0w0UXpRl92LxEhN5mooHefUIQaRvPW1S8TrL1d/+M8WxLeKT11\neDV7MyiZBqir0h1b8UVt0dSuUg/0vkxlhEOFIjTP8toUYxZ8m++HrHjPbI0qQyBD\nLw65fqZ/bksisBxPBjmmc6A=\n-----END PRIVATE KEY-----\n",
                client_email = "promoclicks-185313@appspot.gserviceaccount.com",
                client_id = "",
                auth_uri = "https://accounts.google.com/o/oauth2/auth",
                token_uri = "https://oauth2.googleapis.com/token",
                auth_provider_x509_cert_url = "https://www.googleapis.com/oauth2/v1/certs",
                client_x509_cert_url = "https://www.googleapis.com/robot/v1/metadata/x509/promoclicks-185313%40appspot.gserviceaccount.com",
                universe_domain = "googleapis.com"

            };
            var credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(credenciais));
            // Inject the Cloud Vision scopes 
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped(new[]
                {
                    VisionService.Scope.CloudPlatform
                });
            }
            return new VisionService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                GZipEnabled = false
            });
        }
    }
}