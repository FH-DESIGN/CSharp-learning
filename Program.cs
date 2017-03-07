using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Timers;
using NAudio;
using NAudio.Wave;

namespace MusicPlayer
{
    class Program
    {

        static string DIRECTORY = @"E:\Music";
        static List<string> songsList;
        static int songNum;
        static string songName;
        static WaveOutEvent waveOut;
        static MediaFoundationReader reader;


        static bool playMode;
        static bool orderLoopMode;

        static bool isSelected;
        static bool isLegalNum;

        static PlaybackState state;

        //ERRORS
        enum ERRORS
        {
            ERRORS_OPTION_IS_ILLEGAL, 
            ERRORS_OPTION_IS_LOWER_THAN_ZERO,
            ERRORS_OPTION_IS_LARGER_THAN_SONGS_QUANTITY,
        }

        static string[] ERRORS_COLLECTION =
        {
            "Option number is illegal.",
            "Option number is lower than 0.",
            "Option number is larger than songs' quantity."

        };




        static void Main(string[] args)
        {



            //Basic User Interface init
            MainPage();

            //songs list. Add songs to the song list;
            songsList = new List<string>();
            songsList.Clear();
            CreateSongList(songsList);

            //song name
            songName = "";

            //songs quantity;
            int songQuantity = songsList.Count();


            //select option flags;
            string option = "";
            isSelected = false;

            //song selector
            string getNum = "";
            songNum = 0;
            string getOpt = "";
            isLegalNum = false;


            //create waveoutEvent
            waveOut = new WaveOutEvent();
            waveOut.PlaybackStopped += OnPlaybackStopped;

            //player Manager;


            //AUTORESETEVENT

            playMode = false;

            orderLoopMode = false;
           




            //program running state;
            bool isRunning = false;

            //start program
            isRunning = true;

            while (isRunning) //Main loop
            {
                while (!isSelected)
                {

                    option = Console.ReadLine();

                    switch (option)
                    {
                        case "1":
                            Console.Clear();
                            PlaybackUsage(songQuantity);
                            ShowPlaylist(songsList);
                            isSelected = true;
                            //isLegalNum = false;
                            
                            break;
                        case "2":
                            isSelected = true;

                            break;
                        case "q":
                            isSelected = true;
                            
                            isRunning = false;
                            Environment.Exit(0);
                            break;
                        default:
                            printErrors(ERRORS.ERRORS_OPTION_IS_ILLEGAL);
                            isSelected = false;
                            option = "";
                            break;
                    }

                    

                }

                isSelected = false;


                /*opton 1*/
                while (option.Equals("1"))
                {



                    //1.show play list
                    //2.init the function of playing music                       
                    while (!isSelected)
                    {

                        getNum = Console.ReadLine();
                        
                        
                        switch (getNum)
                        {
                            case "b":
                                option = "";
                                GoMainPage();
                                isSelected = true;
                                isLegalNum = false;
                                break;
                            case "o":
                                Console.WriteLine("Enable list loop mode.");
                                
                                orderLoopMode = true;
                                isLegalNum = true;
                                isSelected = true;
                                songNum =1;
                                break;
                            case "p":
                                isSelected = true;
                                isLegalNum = true;
                                if(songNum == 0)
                                {
                                    songNum = 1;
                                }
                                break;
                            default:
                                if(int.TryParse(getNum,out songNum)){
                                    songNum = int.Parse(getNum);
                                    if (songNum <= 0)
                                    {
                                        printErrors(ERRORS.ERRORS_OPTION_IS_LOWER_THAN_ZERO);
                                        isSelected = false;
                                        //selectError = true;
                                    }
                                    else if (songNum > songQuantity)
                                    {
                                        printErrors(ERRORS.ERRORS_OPTION_IS_LARGER_THAN_SONGS_QUANTITY);
                                        isSelected = false;
                                        //selectError = true;
                                    }
                                    else
                                    {
                                        isSelected = true;
                                        isLegalNum = true;;
                                        //getOpt = "p";
                                    }
                                }
                                else
                                {
                                    printErrors(ERRORS.ERRORS_OPTION_IS_ILLEGAL);
                                    
                                    isSelected = false;
                                    //selectError = true;
                                }
                                //Console.WriteLine("default is over");
                                break;
                        }

                        //Console.WriteLine("new area");
                        

                    } // while(!isSelected);
                    //getNum = "";

                    if (isLegalNum)
                    {
  

                        songName = songsList[songNum - 1].Substring(9);
                        state = PlaybackState.Stopped;

                        Thread thread = new Thread(PrintCurrentTime);
                        
                        /*if(waveOut == null)
                        {
                            waveOut = new WaveOutEvent();
                            waveOut.PlaybackStopped += OnPlaybackStopped;

                        }*/
                        reader = new MediaFoundationReader(songsList[songNum - 1]);
                        waveOut.Init(reader);
                        playMode = true;
                        getOpt = "p";
                        PrintSign(50, "-");
                        while (playMode)
                        {
                            

                            if (getOpt.Equals(""))
                            {
                                getOpt = Console.ReadLine();
                            }


                            switch (getOpt)
                            {
                                case "b":
                                    Console.Clear();
                                    waveOut.Stop();
                                    if(reader != null)
                                    {
                                        reader.Dispose();
                                        reader = null;
                                    }
                                    if(waveOut != null)
                                    {
                                        waveOut.Dispose();
                                        //waveOut = null;
                                    }
                                    GoMainPage();
                                    isSelected = true;
                                    isLegalNum = false;
                                    playMode = false;
                                    songNum = 0;
                                    getNum = "";
                                    option = "";
                                    break;
                                case "c":
                                    Console.Clear();
                                    PlaybackUsage(songQuantity);
                                    ShowPlaylist(songsList);
                                    break;
                                case "o":
                                    
                                    if (!orderLoopMode)
                                    {
                                        orderLoopMode = true;
                                        Console.WriteLine("Enable list loop mode.");
                                    }
                                    else
                                    {
                                        orderLoopMode = false;
                                        Console.WriteLine("Disable list loop mode.");
                                    }
                                    
                                    //songNum = 1;
                                    break;
                                case "p":
                                    if (!state.Equals(PlaybackState.Playing))
                                    {
                                        waveOut.Play();
                                        state = PlaybackState.Playing;
                                        

                                        Console.WriteLine("Playing: {0} Total time: {1}:{2}:{3}", songName, reader.TotalTime.Hours, reader.TotalTime.Minutes, reader.TotalTime.Seconds);
                                        if (thread.ThreadState.Equals(ThreadState.Unstarted))
                                        {
                                            thread.Start();
                                        }else if (thread.ThreadState.Equals(ThreadState.Stopped))
                                        {
                                            thread = new Thread(PrintCurrentTime);
                                            thread.Start();
                                        }
                                        
                                    }


                                    break;
                                case "s":
                                    state = PlaybackState.Stopped;
                                    orderLoopMode = false;
                                    waveOut.Stop();
                                    if(reader != null)
                                    {
                                        reader.Dispose();
                                        
                                        reader = null;
                                    }
                                    if(waveOut != null)
                                    {
                                        waveOut.Dispose();
                                        //waveOut = null;
                                    }
                                    
                                    
                                    Console.Clear();
                                    PlaybackUsage(songQuantity);
                                    ShowPlaylist(songsList);
                                    Console.WriteLine("Stopped: {0}.", songName);
                                    
                                    
                                    playMode = false;
                                    isSelected = false;
                                    //songNum = 0;
                                    getNum = "";

                                    break;
                                case "h":
                                    if (state.Equals(PlaybackState.Playing))
                                    {

                                        waveOut.Pause();
                                        state = PlaybackState.Paused;
                                        Console.WriteLine("Paused: {0}.", songName);

                                    }

                                    break;

                                default:

                                    if (int.TryParse(getOpt, out songNum))
                                    {
                                        waveOut.Stop();
                                        if(waveOut != null)
                                        {
                                            waveOut.Dispose();
                                            //waveOut = null;
                                        }

                                        if(reader != null)
                                        {
                                            reader.Dispose();
                                            reader = null;
                                            
                                        }
                                        

                                        int tempSongNum = int.Parse(getOpt);
                                        if (!(tempSongNum < 0) && !(tempSongNum > songQuantity))
                                        {
                                            songNum = tempSongNum;
                                            reader = new MediaFoundationReader(songsList[songNum - 1]);
                                        }
                                        playMode = false;
                                        isSelected = true;
                                        
                                        
                                    }
                                    else
                                    {
                                        printErrors(ERRORS.ERRORS_OPTION_IS_ILLEGAL);
                                        Console.WriteLine("Please try again.");
                                    }
                                    break;


                            }

                            //Console.WriteLine(reader.TotalTime.Subtract(reader.CurrentTime));
                            
                            getOpt = "";
                            /*if (autoplay)
                            {
                                Thread.Sleep(1000*30);
                                
                                getOpt = "p";
                            }*/

                        }

                    }





                    

                    
                } //while option == 1


            } //isRunning




            Console.ReadLine();
            Console.ReadLine();



        }

        private static void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            Console.WriteLine("Stopped: {0}", songName);
            state = PlaybackState.Stopped;
            if (orderLoopMode)
            {

                
                songNum = songNum + 1;
                Console.WriteLine(songNum);
                if (songNum > 24)
                {
                    songNum = 1;
                    
                }

                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }

                if (waveOut != null)
                {
                    waveOut.Dispose();
                    //waveOut = null;
                }


                
                //waveOut = new WaveOutEvent();
                //waveOut.PlaybackStopped += OnPlaybackStopped;
                    reader = new MediaFoundationReader(songsList[songNum-1]);
                songName = songsList[songNum - 1].Substring(songsList[songNum - 1].LastIndexOf("\\")+1);
                waveOut.Init(reader);
                    waveOut.Play();
                state = PlaybackState.Playing;
                Console.WriteLine("Playing: {0} Total time: {1}:{2}:{3}", songName,reader.TotalTime.Hours,reader.TotalTime.Minutes,reader.TotalTime.Seconds);


            }




        }

        static void PrintCurrentTime()
        {
            while (state.Equals(PlaybackState.Playing))
            {
                Console.Clear();
                PlaybackUsage(24);
                ShowPlaylist(songsList);
                
                //for(int i = 0; i < (long)reader.TotalTime.Seconds; i++)
                //{
                    Console.WriteLine("{0}:{1}:{2}", reader.CurrentTime.Hours, reader.CurrentTime.Minutes, reader.CurrentTime.Seconds);
                    //Console.Write(">");
                    Thread.Sleep(1000);
                //}
                //Console.WriteLine();
                
            }
            
        }






        /**********************************************************************************************************************************
         * UI Design
         * 1.Main Page
         * 2.Playback Page
         *
         *********************************************************************************************************************************/

        //Print sign(examples:- + * =...);
        static void PrintSign(int num, string sign)
        {

            for (int i = 0; i < num; i++)
            {
                Console.Write(sign);
            }

            Console.WriteLine();

        }

        //Go main page
        static void GoMainPage()
        {
            Console.Clear();
            MainPage();
        }

        
        static void MainPageTitle()
        {
            string name = "OpenMusic";
            string version = "V1.0 Beta";
            Console.WriteLine("{0} {1}",name,version);
        }

        static void MainPageFunc()
        {

            Console.WriteLine("      1| PlayList");
            Console.WriteLine("      2| Add...");

        }

        static void MainPageUsage()
        {

            Console.WriteLine("Usage: ");
            Console.WriteLine("      [ # ]: Give a choive.");
            Console.WriteLine("      [ q ]: Exit.");

        }

        static void Copyright()
        {
            Console.WriteLine("@FH-DESIGN");
        }

        static void MainPage()
        {
            //Print title;         
            PrintSign(50, "-");
            MainPageTitle();
            PrintSign(50, "-");
            

            //print main page usage
            MainPageUsage();
            PrintSign(50, "-");

            //print main page functions
            Console.WriteLine();
            MainPageFunc();
            Console.WriteLine();

            //print copyright;
            PrintSign(50, "-");
            Copyright();
            PrintSign(50, "-");

        }


        //Playback page
        static string[] GetFiles(string path,string fileType)
        {
            string[] files;
            files = Directory.GetFiles(path, fileType);
            return files;

        }

        static List<string> CreateSongList(List<string> list)//add songs to the songslist;
        {

            string[] mp3files = GetFiles(DIRECTORY, "*.mp3");
            string[] wavfiles = GetFiles(DIRECTORY, "*.wav");

            for (int i = 0; i < mp3files.Length; i++)
            {
                list.Add(mp3files[i]);
            }

            for(int i = 0; i < wavfiles.Length; i++)
            {
                list.Add(wavfiles[i]);
            }

            return list;

        }

        static void ShowPlaylist(List<string> list) //show playlist
        {
            Console.WriteLine("Playlist:");

            int number = 1;
            int header = 0;
            int tail = 0;
            string song;
            string type;

            foreach (string item in list)
            {

                header = item.LastIndexOf('\\');
                tail = item.LastIndexOf('.');
                song = item.Substring(header + 1, tail - header - 1);
                type = item.Substring(tail + 1);
                Console.WriteLine("          " + number + "" + "\t" + "|" + " " + song);
                number++;

            }
            PrintSign(50, "-");
        }
        static void PlaybackUsage(int amount)
        {
            
            PrintSign(50, "-");
            Console.WriteLine("Usage: ");
            Console.WriteLine("          [ c ]: Clean the screen.");
            Console.WriteLine("          [ o ]: Enable list loop mode. Re-type to disable");
            Console.WriteLine("          [ s ]: Stop");
            Console.WriteLine("          [ p ]: Play");
            Console.WriteLine("          [ h ]: Pause");
            Console.WriteLine("          [ # ]: selector a song.<{0} to {1}> ", 1, amount);
            PrintSign(50, "-");

        }



        //printErrors
        static void printErrors(ERRORS error)
        {
            int errorNum = (int)error;

            Console.WriteLine("Error {0}:{1}",errorNum,ERRORS_COLLECTION[errorNum]);

        }

    }

}
