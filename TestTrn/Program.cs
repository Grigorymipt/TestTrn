using QBitNinja.Client;
using QBitNinja.Client.Models;
using NBitcoin;
using DocumentFormat.OpenXml.Presentation;
using System.Text.Json;


var bitcoinPrivateKey = new BitcoinSecret("cSnRPdoymVgsXScvBQh9XyCrfhAo7xYMAewMsAY5sdhi24fH9V44", Network.TestNet);
var network = bitcoinPrivateKey.Network;
var address = bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Legacy);

Console.WriteLine(bitcoinPrivateKey); // cSnRPdoymVgsXScvBQh9XyCrfhAo7xYMAewMsAY5sdhi24fH9V44
Console.WriteLine(address); // mg2UyNctXECoY4NCNPwzvajeuiAHp1MYyo

var client = new QBitNinjaClient(network);
var transactionId = uint256.Parse("a4e6a53d519f77e9efdcc1082cd3a06d454b9f2bb89d1fc1f9cddd67d0c36138");
var transactionResponse = client.GetTransaction(transactionId).Result;

Console.WriteLine(transactionResponse.TransactionId); // a4e6a53d519f77e9efdcc1082cd3a06d454b9f2bb89d1fc1f9cddd67d0c36138
Console.WriteLine(transactionResponse.Block.Confirmations); // 91
//*******

var publicKeyHash = new KeyId("14836dbe7f38c5ac3d49e8d790af808a4ee9edcf");

var testNetAddress = publicKeyHash.GetAddress(Network.TestNet);
var mainNetAddress = publicKeyHash.GetAddress(Network.Main);

Console.WriteLine(mainNetAddress.ScriptPubKey); // OP_DUP OP_HASH160 14836dbe7f38c5ac3d49e8d790af808a4ee9edcf OP_EQUALVERIFY OP_CHECKSIG
Console.WriteLine(testNetAddress.ScriptPubKey);

//*****

var receivedCoins = transactionResponse.ReceivedCoins;


OutPoint outPointToSpend = null;
foreach (var coin in receivedCoins)
{
    if (coin.TxOut.ScriptPubKey == bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Legacy).ScriptPubKey)
    {
        outPointToSpend = coin.Outpoint;
    }
}
if (outPointToSpend == null)
    throw new Exception("TxOut doesn't contain our ScriptPubKey");
Console.WriteLine("We want to spend {0}. outpoint:", outPointToSpend.N + 1);

var transaction = Transaction.Create(network);

Console.WriteLine(transaction.GetHash());
transaction.Inputs.Add(new TxIn()
{
    PrevOut = outPointToSpend
});
Console.WriteLine(transaction.GetHash());

var hallOfTheMakersAddress = BitcoinAddress.Create("tb1ql7w62elx9ucw4pj5lgw4l028hmuw80sndtntxt", Network.TestNet);

//transaction.Outputs.Add(Money.Coins(0.000001m), hallOfTheMakersAddress.ScriptPubKey);
//transaction.Outputs.Add()
transaction.Outputs.Add(new TxOut()
{
    Value = Money.Zero,
    
    //ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes),
});
// Send the change back
// transaction.Outputs.Add(new Money(0.000001m, MoneyUnit.BTC), bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Legacy).ScriptPubKey);


// Sign Trn
transaction.Inputs[0].ScriptSig = bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Legacy).ScriptPubKey;
transaction.Sign(bitcoinPrivateKey, receivedCoins.ToArray());
BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;

if (!broadcastResponse.Success)
{
    Console.Error.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
    Console.Error.WriteLine("Error message: " + broadcastResponse.Error.Reason);
}
else
{
    Console.WriteLine("Success! You can check out the hash of the transaciton in any block explorer:");
    Console.WriteLine(transaction.GetHash());
}
/*
Trn trn = new Trn("Tom", 37);
string json = JsonSerializer.Serialize(trn);
Console.WriteLine(json);
Trn? restoredPerson = JsonSerializer.Deserialize<Trn>(json);
Console.WriteLine(restoredPerson?.Name); // Tom

class Trn
{
    public string Name { get; }
    public int Age { get; set; }
    public Trn(string name, int age)
    {
        Name = name;
        Age = age;
    }
}
*/
