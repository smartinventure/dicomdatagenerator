using DicomDataGenerator.Models;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;

namespace DicomDataGenerator.Services
{
    /// <summary>Sends generated instances to a PACS via C-STORE (one association per call/batch).</summary>
    public class PacsSender
    {
        public async Task SendAsync(IReadOnlyList<DicomFile> files, PacsOptions pacs, CancellationToken cancellationToken)
        {
            if (files.Count == 0)
            {
                return;
            }
            var client = DicomClientFactory.Create(pacs.Host, pacs.Port, false, pacs.CallingAet, pacs.CalledAet);
            foreach (var file in files)
            {
                await client.AddRequestAsync(new DicomCStoreRequest(file)).ConfigureAwait(false);
            }
            await client.SendAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Verifies connectivity to the PACS with a C-ECHO (DICOM "ping"). Times out after ~10s.</summary>
        public async Task<bool> EchoAsync(PacsOptions pacs, CancellationToken cancellationToken)
        {
            var client = DicomClientFactory.Create(pacs.Host, pacs.Port, false, pacs.CallingAet, pacs.CalledAet);
            var success = false;
            var echo = new DicomCEchoRequest
            {
                OnResponseReceived = (_, response) => success = response.Status == DicomStatus.Success
            };
            await client.AddRequestAsync(echo).ConfigureAwait(false);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));
            await client.SendAsync(timeout.Token).ConfigureAwait(false);
            return success;
        }
    }
}
