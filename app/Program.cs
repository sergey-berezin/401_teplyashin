using ClassLibrary2;

Class1 cls = new Class1();

CancellationTokenSource source_1 = new CancellationTokenSource();
CancellationToken token_1 = source_1.Token;
CancellationTokenSource source_2 = new CancellationTokenSource();
CancellationToken token_2 = source_2.Token;
CancellationTokenSource source_3 = new CancellationTokenSource();
CancellationToken token_3 = source_3.Token;

var img_1 = cls.process("../../../../img/happiness.png", token_1);
var img_2 = cls.process("../../../../img/fear.jpg", token_2);
var img_3 = cls.process("../../../../img/anger.jpg", token_3);

source_2.Cancel();

await img_1;
await img_2;
await img_3;

string str_1 = "";
Console.WriteLine("img1");
for (int i = 0; i < img_1.Result.Length; i++)
    str_1 += img_1.Result[i].Item1 + " " + img_1.Result[i].Item2.ToString() + "\n";
Console.WriteLine(str_1);

string str_2 = "";
Console.WriteLine("img2");
for (int i = 0; i < img_2.Result.Length; i++)
    str_2 += img_2.Result[i].Item1 + " " + img_2.Result[i].Item2.ToString() + "\n";
Console.WriteLine(str_2);

string str_3 = "";
Console.WriteLine("img3");
for (int i = 0; i < img_3.Result.Length; i++)
    str_3 += img_3.Result[i].Item1 + " " + img_3.Result[i].Item2.ToString() + "\n";
Console.WriteLine(str_3);