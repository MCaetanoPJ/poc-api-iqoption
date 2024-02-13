using System.Reactive.Linq;
using IqOptionApiDotNet;
using IqOptionApiDotNet.Models;

try
{
    var client = new IqOptionApiDotNetClient("your_email", "your_password");

    var conectado = await client.ConnectAsync();

    if (!conectado)
    {
        Console.WriteLine("Não conectado . . .");
        return;
    }

    var requestId = Guid.NewGuid().ToString().Replace("-", string.Empty); // New
    var profile = await client.GetProfileAsync(requestId);

    var GetTypes = new List<BalanceType>();
    GetTypes.Add(BalanceType.Practice);
    GetTypes.Add(BalanceType.Real);

    while (true)
    {
        requestId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var balanceBeforeAsync = await client.GetBalancesAsync(requestId, GetTypes);
        var balanceBefore = balanceBeforeAsync.FirstOrDefault(c => c.Type == BalanceType.Practice);
        var moneyBefore = balanceBefore.Amount;

        requestId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var timeExpiration = DateTimeOffset.Now.AddMinutes(1);
        var buyResult = await client.WsClient.BuyAsync(requestId, BalanceType.Practice, ActivePair.EURUSD_OTC, 2, OrderDirection.Call, timeExpiration);
        if (buyResult.PositionId == null)
        {
            Console.WriteLine($"Não é possível enviar a ordem: {buyResult.ErrorMessage}");
            Console.WriteLine($"Aguardando 1m até a próxima ordem ser enviada.");
            await Task.Delay(60000);
        }
        else
        {
            var expiraEm = (buyResult.Exp - buyResult.Created).TotalMilliseconds;
            var expiraInt = Convert.ToInt32(expiraEm);

            await Task.Delay(expiraInt);

            requestId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var balanceAfterAsync = await client.GetBalancesAsync(requestId, GetTypes);
            var balanceAfter = balanceAfterAsync.FirstOrDefault(c => c.Type == BalanceType.Practice);
            var moneyAfter = balanceAfter.Amount;

            var valorAtual = moneyAfter - moneyBefore;

            var winOrLose = moneyBefore < moneyAfter;
            var resultado = winOrLose ? "ganhou" : "perdeu";

            Console.WriteLine($"Horário: {DateTime.Now.ToString("HH:mm")} - Valor anterior: {moneyBefore.ToString("C2")} - Valor atual: {moneyAfter.ToString("C2")} -  Você {resultado}: {valorAtual.ToString("C2")}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro desconhecido: {ex.Message}");
}