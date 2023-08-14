using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Steam.Models.SteamCommunity;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SysFile = System.IO.File;
using System.Diagnostics;
using System.Net;

namespace StreamUserList;

internal sealed class AppConfig
{
    public string SteamApiKeys { get; set; }

    public int Rus { get; set; }

    public int Chinese { get; set; }

    public HttpProxyConfig[] Proxies {get; set; }
}

internal sealed class HttpProxyConfig
{
    public string Host { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }
}

internal class Program
{
    public static Regex ChineseRegex = new(@"[\u4E00-\u9FFF]");
    public static Regex RuRegex = new(@"[\u0400-\u04FF]");
    private static readonly object writerLock = new object();

    public static async Task Main(string[] args)
    {
        var appSettings = await SysFile.ReadAllTextAsync("app-settings.json");
        var ids = await SysFile.ReadAllLinesAsync("check_ids.txt");
        ulong[] parsedIds = null;

        try
        {
            parsedIds = ids.Select(x => ulong.Parse(x.Trim())).ToArray();
        }
        catch
        {
            Console.WriteLine("Не могу расспарсить Ids из файла check_ids.txt");
        }

        var options = JsonConvert.DeserializeObject<AppConfig>(appSettings);

        if (parsedIds == null)
        {
            Console.WriteLine("Что то пошло не так с ids.");
            return;
        }

        var splitKeys = options.SteamApiKeys.Split(',').ToArray();

        if (splitKeys.Length != 5)
        {
            await Console.Out.WriteLineAsync("Что то не так с API Keys. Проверте что они передаются через запятую.");
            return;
        }

        if (options.Proxies.Length != 5)
        {
            await Console.Out.WriteLineAsync("Что то не так проксями. Проверте секцию Proxies:[....] в app-settings.json.");
        }

        (ulong[] bunch1, 
         ulong[] bunch2,
         ulong[] bunch3,
         ulong[] bunch4,
         ulong[] bunch5) = DistributeIds(parsedIds);

        var webInterfaceFactory1 = new SteamWebInterfaceFactory(splitKeys[0]);
        var webInterfaceFactory2 = new SteamWebInterfaceFactory(splitKeys[1]);
        var webInterfaceFactory3 = new SteamWebInterfaceFactory(splitKeys[2]);
        var webInterfaceFactory4 = new SteamWebInterfaceFactory(splitKeys[3]);
        var webInterfaceFactory5 = new SteamWebInterfaceFactory(splitKeys[4]);

        if (!Directory.Exists("Friends_Chinese"))
        {
            Directory.CreateDirectory("Friends_Chinese");
        }

        if (!Directory.Exists("Friends_Ru"))
        {
            Directory.CreateDirectory("Friends_Ru");
        }

        if (!Directory.Exists("Friends_Burjui"))
        {
            Directory.CreateDirectory("Friends_Burjui");
        }

        if (!Directory.Exists("Friends_No_Friends"))
        {
            Directory.CreateDirectory("Friends_No_Friends");
        }

        var now = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        await using (var selfChineseWriter = File.CreateText($"Self_Chinese_{now}.txt"))
        await using (var selfRuWriter = File.CreateText($"Self_Ru_{now}.txt"))
        await using (var selfBurjuiWriter = File.CreateText($"Self_Burjui_{now}.txt"))
        await using (var chineseWriter = File.CreateText($"Friends_Chinese/Chinese_{now}.txt"))
        await using (var ruWriter = File.CreateText($"Friends_Ru/Ru_{now}.txt"))
        await using (var burjuiWriter = File.CreateText($"Friends_Burjui/Burjui_{now}.txt"))
        await using (var noFriendsWriter = File.CreateText($"Friends_No_Friends/No_Friends_{now}.txt"))
        {
            Task task1 = Task.Run(() => AnalyzeIds(
                bunch1,
                webInterfaceFactory1,
                new HttpClient(new HttpClientHandler
                {
                    Proxy = new WebProxy(options.Proxies[0].Host)
                    {
                        Credentials = new NetworkCredential(options.Proxies[0].UserName, options.Proxies[0].Password)
                    },
                    UseProxy = true,
                    PreAuthenticate = true,
                    UseDefaultCredentials = false,
                    //ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                }) { Timeout = TimeSpan.FromSeconds(10) },
                options,
                selfChineseWriter,
                selfRuWriter,
                selfBurjuiWriter,
                ruWriter,
                chineseWriter,
                burjuiWriter,
                noFriendsWriter));

            Task task2 = Task.Run(() => AnalyzeIds(
                bunch2,
                webInterfaceFactory2,
                new HttpClient(new HttpClientHandler
                {
                    Proxy = new WebProxy(options.Proxies[1].Host)
                    {
                        Credentials = new NetworkCredential(options.Proxies[1].UserName, options.Proxies[1].Password)
                    },
                    UseProxy = true,
                    PreAuthenticate = true,
                    UseDefaultCredentials = false,
                    //ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                }) { Timeout = TimeSpan.FromSeconds(10) },
                options,
                selfChineseWriter,
                selfRuWriter,
                selfBurjuiWriter,
                ruWriter,
                chineseWriter,
                burjuiWriter,
                noFriendsWriter));

            Task task3 = Task.Run(() => AnalyzeIds(
                bunch3,
                webInterfaceFactory3,
                new HttpClient(new HttpClientHandler
                {
                    Proxy = new WebProxy(options.Proxies[2].Host)
                    {
                        Credentials = new NetworkCredential(options.Proxies[2].UserName, options.Proxies[2].Password)
                    },
                    UseProxy = true,
                    PreAuthenticate = true,
                    UseDefaultCredentials = false,
                    //ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                }) { Timeout = TimeSpan.FromSeconds(10) },
                options,
                selfChineseWriter,
                selfRuWriter,
                selfBurjuiWriter,
                ruWriter,
                chineseWriter,
                burjuiWriter,
                noFriendsWriter));

            Task task4 = Task.Run(() => AnalyzeIds(
                bunch4,
                webInterfaceFactory4,
                new HttpClient(new HttpClientHandler
                {
                    Proxy = new WebProxy(options.Proxies[3].Host)
                    {
                        Credentials = new NetworkCredential(options.Proxies[3].UserName, options.Proxies[3].Password)
                    },
                    UseProxy = true,
                    PreAuthenticate = true,
                    UseDefaultCredentials = false,
                    //ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                }) { Timeout = TimeSpan.FromSeconds(10) },
                options,
                selfChineseWriter,
                selfRuWriter,
                selfBurjuiWriter,
                ruWriter,
                chineseWriter,
                burjuiWriter,
                noFriendsWriter));

            Task task5 = Task.Run(() => AnalyzeIds(
                bunch5,
                webInterfaceFactory5,
                new HttpClient(new HttpClientHandler
                {
                    Proxy = new WebProxy(options.Proxies[4].Host)
                    {
                        Credentials = new NetworkCredential(options.Proxies[4].UserName, options.Proxies[4].Password)
                    },
                    
                    UseProxy = true,
                    PreAuthenticate = true,
                    UseDefaultCredentials = false,
                    //ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                }) { Timeout = TimeSpan.FromSeconds(10) },
                options,
                selfChineseWriter,
                selfRuWriter,
                selfBurjuiWriter,
                ruWriter,
                chineseWriter,
                burjuiWriter,
                noFriendsWriter));

            await Task.WhenAll(task1, task2, task3, task4, task5);
        }
       
        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        Console.WriteLine($"Time: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}");
    }

    public static bool ContainsChineseCharacters(string input)
    {
        return ChineseRegex.IsMatch(input);
    }

    public static bool ContainsRuCharacters(string input)
    {
        return RuRegex.IsMatch(input);
    }

    public static async Task AnalyzeIds(
        ulong[] parsedIds,
        SteamWebInterfaceFactory webInterfaceFactory,
        HttpClient httpClient,
        AppConfig appConfig,
        StreamWriter selfChineseWriter,
        StreamWriter selfRuWriter,
        StreamWriter selfBurjuiWriter,
        StreamWriter ruWriter,
        StreamWriter chineseWriter,
        StreamWriter burjuiWriter,
        StreamWriter noFriendsWriter)
    {
        var steamUserInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(httpClient);

        foreach (var id in parsedIds)
        {
            Console.Out.WriteLine($"{id}");
            var chineses = new List<ulong>();
            var russes = new List<ulong>();

            await Task.Delay(350);

            try
            {
                ISteamWebResponse<IReadOnlyCollection<FriendModel>> friendsListResponse =
                    await steamUserInterface.GetFriendsListAsync(id);

                if (friendsListResponse.Data.Count == 0)
                {
                    lock (writerLock)
                    {
                        noFriendsWriter.WriteLine($"{id} ");
                    }
                    continue;
                }

                foreach (var friend in friendsListResponse.Data)
                {
                    await Task.Delay(100);

                    var playerSummaryResponse = await steamUserInterface.GetPlayerSummaryAsync(friend.SteamId);

                    await Task.Delay(120);

                    if (playerSummaryResponse is not { Data: not null }
                        || string.IsNullOrWhiteSpace(playerSummaryResponse.Data.Nickname))
                    {
                        continue;
                    }

                    bool isChinese = ContainsChineseCharacters(playerSummaryResponse.Data.Nickname);
                    bool isRu = ContainsRuCharacters(playerSummaryResponse.Data.Nickname);

                    if (isChinese && isRu)
                    {
                        lock (writerLock)
                        {
                            ruWriter.WriteLine($"{friend.SteamId} ");
                        }
                        russes.Add(id);
                    }

                    if (isRu && !isChinese)
                    {
                        lock (writerLock)
                        {
                            ruWriter.WriteLine($"{friend.SteamId} ");
                        }
                        russes.Add(id);
                    }

                    if (isChinese && !isRu)
                    {
                        lock (writerLock)
                        {
                            chineseWriter.WriteLine($"{friend.SteamId} ");
                        }
                        chineses.Add(id);
                    }

                    if (!isChinese && !isRu)
                    {
                        lock (writerLock)
                        {
                            burjuiWriter.WriteLine($"{friend.SteamId} ");
                        }
                    }
                }

                //Add code to self check.
                if (russes.Count < appConfig.Rus && chineses.Count < appConfig.Chinese)
                {
                    lock (writerLock)
                    {
                        selfBurjuiWriter.WriteLine($"{id}");
                    }
                }

                else if (russes.Count >= appConfig.Rus && chineses.Count < appConfig.Chinese)
                {
                    lock (writerLock)
                    {
                        selfRuWriter.WriteLine($"{id}");
                    }
                    continue;
                }

                if (chineses.Count >= appConfig.Chinese && russes.Count < appConfig.Rus)
                {
                    lock (writerLock)
                    {
                        selfChineseWriter.WriteLine($"{id}");
                    }
                    continue;
                }

                if (chineses.Count >= appConfig.Chinese && russes.Count >= appConfig.Rus)
                {
                    lock (writerLock)
                    {
                        selfChineseWriter.WriteLine($"{id}");
                    }
                }

                await Task.Delay(300);
            }
            catch (Exception ex)
            {
                lock (writerLock)
                {
                    noFriendsWriter.WriteLine($"{id}");
                    noFriendsWriter.Flush();
                }
                await Task.Delay(50);
            }
        }
    }

    public static (ulong[], ulong[], ulong[], ulong[], ulong[]) DistributeIds(ulong[] ids)
    {
        // Calculate the number of elements to be placed in each array
        int totalElements = ids.Length;
        int arraysCount = 5;
        int elementsPerArray = totalElements / arraysCount;
        int remainingElements = totalElements % arraysCount;

        // Create five independent arrays
        ulong[] array1 = new ulong[elementsPerArray + (remainingElements > 0 ? 1 : 0)];
        ulong[] array2 = new ulong[elementsPerArray + (remainingElements > 1 ? 1 : 0)];
        ulong[] array3 = new ulong[elementsPerArray];
        ulong[] array4 = new ulong[elementsPerArray];
        ulong[] array5 = new ulong[elementsPerArray];

        // Distribute elements among the arrays
        int currentIndex = 0;

        for (int i = 0; i < array1.Length; i++)
        {
            array1[i] = ids[currentIndex++];
        }

        for (int i = 0; i < array2.Length; i++)
        {
            array2[i] = ids[currentIndex++];
        }

        for (int i = 0; i < array3.Length; i++)
        {
            array3[i] = ids[currentIndex++];
        }

        for (int i = 0; i < array4.Length; i++)
        {
            array4[i] = ids[currentIndex++];
        }

        for (int i = 0; i < array5.Length; i++)
        {
            array5[i] = ids[currentIndex++];
        }

        return (array1, array2, array3, array4, array5);
    }
}
