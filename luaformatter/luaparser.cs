using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace luaformatter
{
    public class luaparser
    {
        public static List<LuaDefination> defs = new List<LuaDefination>();

        static luaparser()
        {
            /* LuaDefination class defines a regular expression to search for and an index offset (if needed).
            * index offset gets added to the regex match index.
            * 
            * ex - text in lua file
            * local t = doTaskInTime(function() dosomething() end)end
            * 
            * the ending ")end" is matched with this regex @"\)end"
            * the index offset is 1 so the ')' will be left behind and the text split at "end" instead of ")end"
            * leaving the ')' behind where if should be
            * 
            * with offset index 1
            * local t = doTaskInTime(
            *      function() 
            *          dosomething() 
            *      end)
            * end
            * 
            * without offset index
            * local t = doTaskInTime(
            *      function() 
            *          dosomething() 
            *      end
            *      )end  - note the tab alignment will be off too
            * 
            * so the splits happens at "end" insteat of ")end"
           */
            defs.Add(new LuaDefination() { regex = new Regex("local (?<!^local function .*)") });            // local - not local function from the beginning of line
            defs.Add(new LuaDefination() { regex = new Regex("return") });
            defs.Add(new LuaDefination() { regex = new Regex("function ") });
            defs.Add(new LuaDefination() { regex = new Regex("local function ") });
            defs.Add(new LuaDefination() { regex = new Regex("if .*then(?!.*elseif.*)") });                  // match if .*then afterwards match .*elseif.* if not found success = true
            defs.Add(new LuaDefination() { regex = new Regex("if .* then(?!.*elseif.*)") });                 // same as above
            defs.Add(new LuaDefination() { regex = new Regex("while .*do") });
            defs.Add(new LuaDefination() { regex = new Regex("while .* do") });
            defs.Add(new LuaDefination() { regex = new Regex("for .* do") });
            defs.Add(new LuaDefination() { regex = new Regex(@"for .*\)do") });
            defs.Add(new LuaDefination() { regex = new Regex(@"\)end"), indexoffset = 1 });
            defs.Add(new LuaDefination() { regex = new Regex(@"(end)$") });                                   // match end at the end of a line
            defs.Add(new LuaDefination() { regex = new Regex(@"\(function\("), indexoffset = 1 });
            defs.Add(new LuaDefination() { regex = new Regex(@",function\("), indexoffset = 1 });
            defs.Add(new LuaDefination() { regex = new Regex(@"\)else(?!.*elseif.*)"), indexoffset = 1 });
            defs.Add(new LuaDefination() { regex = new Regex(@"\)elseif .*then"), indexoffset = 1 });
            defs.Add(new LuaDefination() { regex = new Regex(@"\)elseif .* then"), indexoffset = 1 });
            defs.Add(new LuaDefination() { regex = new Regex(@"elseif .* then") });
            defs.Add(new LuaDefination() { regex = new Regex(@"elseif .*then") });
            defs.Add(new LuaDefination() { regex = new Regex(@"\)end"), indexoffset = 1 });
            defs.Add(new LuaDefination() { regex = new Regex(@";end"), indexoffset = 1 });
            defs.Add(new LuaDefination() { regex = new Regex(@"\}end"), indexoffset = 1 });
            defs.Add(new LuaDefination() { regex = new Regex(@"\)self"), indexoffset = 1 });
            defs.Add(new LuaDefination() { regex = new Regex(@"\}self"), indexoffset = 1 });

            // array decs
            defs.Add(new LuaDefination() { regex = new Regex(@"\{\w*="), indexoffset = 0 });
            defs.Add(new LuaDefination() { regex = new Regex(@",\w*="), indexoffset = 1 });
        }

        public static void _Main(string[] args)
        {
           

            // drag and drop files/folders on exe formats then
            if (args.Length > 0)
            {
                if (Directory.Exists(args[0]))
                {
                    Stopwatch w = new Stopwatch();
                    w.Start();
                    foreach (var c in Directory.GetFiles(args[0], "*.lua", SearchOption.AllDirectories))
                    {
                        formatfile(c, c, ref defs);
                        string fname = c.Split('\\').Last();
                        Console.WriteLine(string.Format("{0} DONE : {1}", fname, w.ElapsedMilliseconds));
                    }
                    w.Stop();
                    Console.WriteLine(string.Format("DONE : {0} misec", w.ElapsedMilliseconds));
                }
                else if (File.Exists(args[0]))
                {
                    Stopwatch w = new Stopwatch();
                    w.Start();
                    formatfile(args[0], args[0], ref defs);
                    string fname = args[0].Split('\\').Last();
                    w.Stop();
                    Console.WriteLine(string.Format("DONE : {0} misec", w.ElapsedMilliseconds));
                }
                else
                {
                    Console.WriteLine("Error : Input file or directory was not found, or did not contain lua files.");
                    Console.ReadLine();
                }
            }

        }

        /// <summary>
        /// Formats/unminfiy the given lua file
        /// </summary>
        /// <param name="path">input file</param>
        /// <param name="output">save path</param>
        public static void formatfile(string path, string output, ref List<LuaDefination> defs)
        {
            string[] flines = File.ReadAllLines(path);
            List<string> nfilelines = new List<string>(flines.Length);

            int lineindex = 0;
          
            string currentline = "";

            Match m = Match.Empty;
            LuaDefination d1 = new LuaDefination();
            LuaDefination d2 = new LuaDefination();

            for (int i = 0; i < flines.Length; i++)
            {
                currentline = flines[i];
                lineindex = i;

                string orgst = currentline;
                string searchtext = currentline;

                int m1index = 0;
                m = Match.Empty;
                bool set = false;
                int m1length = 0;
                d1 = null;
                d2 = null;

                while (true)
                {
                    m1length = 0;

                    // match searchtext with regexs in our defs, keep track of the lowest match index
                    foreach (var c in defs)
                    {
                        Match m2 = c.Match(searchtext);

                        if (m2.Success && (m2.Index <= m.Index || !set))
                        {
                            if (searchtext.Contains('\''))
                            {
                                int mindex = m2.Index;

                                bool good1 = false;
                                bool good2 = false;


                                //search if we are in a string backwards
                                for (int check = mindex; check > -1; check--)
                                {
                                    if (searchtext[check] == '\'')
                                    {
                                        break;
                                    }
                                    else if (searchtext[check] == '=' || searchtext[check] == '(' || searchtext[check] == '{' || searchtext[check] == ',' || searchtext[check] == ')' || searchtext[check] == '}')
                                    {
                                        good1 = true;
                                        break;
                                    }
                                }

                                //search if we are in a string forewards
                                if (!good1)
                                {
                                    for (int check = mindex; check < searchtext.Length; check++)
                                    {
                                        if (searchtext[check] == '\'')
                                        {
                                            break;
                                        }
                                        else if (searchtext[check] == '=' || searchtext[check] == ')' || searchtext[check] == '}' || searchtext[check] == ',' || searchtext[check] == '(' || searchtext[check] == '{')
                                        {
                                            good2 = true;
                                            break;
                                        }
                                    }
                                }

                                if ((good1 && good2) || (good1 && !good2) || (!good1 && good2))
                                {
                                    set = true;
                                    m = m2;
                                    d1 = c;
                                }
                            }
                            else
                            {
                                set = true;
                                m = m2;
                                d1 = c;
                            }
                        }
                    }


                    if (m.Success)
                    {
                        m1length = m.Length;
                        m1index = m.Index;
                        set = false;
                        // total hack, didn't want to spend the time
                        if (m1index < 1)
                            // remove what was matched in m
                            searchtext = searchtext.Remove(m.Index + d1.indexoffset, m.Value.Length -d1.indexoffset);

                        m = Match.Empty;
                        // search again to find the next match in search text (remember m's match was removed)
                        foreach (var c in defs)
                        {
                            Match m2 = c.Match(searchtext);

                            if (m2.Success && (m2.Index <= m.Index || !set))
                            {
                                if (searchtext.Contains('\''))
                                {
                                    int mindex = m2.Index;

                                    bool good1 = false;
                                    bool good2 = false;

                                    for (int check = mindex; check > -1; check--)
                                    {
                                        if (searchtext[check] == '\'')
                                        {
                                            break;
                                        }
                                        else if (searchtext[check] == '=' || searchtext[check] == '=' || searchtext[check] == '(' || searchtext[check] == '{' || searchtext[check] == ',' || searchtext[check] == ')' || searchtext[check] == '}')
                                        {
                                            good1 = true;
                                            break;
                                        }
                                    }

                                    if (!good1)
                                    {
                                        for (int check = mindex; check < searchtext.Length; check++)
                                        {
                                            if (searchtext[check] == '\'')
                                            {
                                                break;
                                            }
                                            else if (searchtext[check] == ')' || searchtext[check] == '}' || searchtext[check] == ',' || searchtext[check] == '(' || searchtext[check] == '{')
                                            {
                                                good2 = true;
                                                break;
                                            }
                                        }
                                    }

                                    if ((good1 && good2) || (good1 && !good2) || (!good1 && good2))
                                    {
                                        set = true;
                                        m = m2;
                                        d2 = c;
                                    }
                                }
                                else
                                {
                                    set = true;
                                    m = m2;
                                    d2 = c;
                                }
                            }
                        }

                        // successfully found a second match and our original search matched at index of 0
                        if (m.Success && m1index < 1)
                        {
                            //remove text from 0 index to our second match + our offset
                            //this removes the current line we just found from search text
                            searchtext = searchtext.Remove(0, m.Index+d2.indexoffset);
                            //extracts the line of lua using our 2 matches
                            string line = orgst.Remove(m.Index + m1length + d1.indexoffset + d2.indexoffset, orgst.Length - m.Index - m1length - d1.indexoffset - d2.indexoffset);
                            // our list of lua lines
                            nfilelines.Add(line);
                            // remove our line  from orgst
                            orgst = orgst.Remove(0, m.Index + m1length + d1.indexoffset + d2.indexoffset);
                            //clean up
                            set = false;
                            m = Match.Empty;
                        }
                        /* Hack solutution for match 1 find a match further that the index of 1
                         * this causes problems in how we split the line
                         * so I cheated and put this in
                         */
                        else if (m1index > 1 && m1length > 0)
                        {
                            searchtext = searchtext.Remove(0, m1index+d1.indexoffset);
                            // remove from match 1 index to end of orgst
                            // basically get our lua line
                            string line = orgst.Remove(m1index+d1.indexoffset, orgst.Length - m1index-d1.indexoffset);
                            nfilelines.Add(line);
                            // remove our lua line from orgst
                            orgst = orgst.Remove(0, m1index+d1.indexoffset);
                            set = false;
                            m = Match.Empty;
                        }
                        // only match 1 found something not 2 therefore we have a valid line, add it to list of lua lines
                        else
                        {
                            m = Match.Empty;
                            nfilelines.Add(orgst);
                            set = false;
                            break;
                        }
                    }
                    else
                    {
                        m = Match.Empty;
                        nfilelines.Add(orgst);
                        set = false;
                        break;
                    }
                    set = false;
                    m = Match.Empty;
                }
            }

            /* stage 2 find variables accessed on a line
             * 
             * ex
             * 
             * local t = getonions()tomato.water=0
             * 
             * these should be on seperat lines
             * 
             * local t = getonions()
             * tomato.water=0
             * 
             * 
            */

            Regex varuse = new Regex(@"\)\w");   // finds ) any letter
            Regex varuse2 = new Regex(@"\}\w");  // finds } any letter
            Regex stringvaruse = new Regex(@"[^=(,\[]'\w(?<!.*require.*)"); // matches any letter other than =(,[ then ' then any letter, if success then tries to match require , if require matches success=false
            Match nm  = Match.Empty;
            Match nm2 = Match.Empty;
            Match nm3 = Match.Empty;

            string or = "";
            string and = "";
            string then = "";
            string sdo = "";

            for (int i = 0; i < nfilelines.Count; i++)
            {
                currentline = nfilelines[i];
                int clength = currentline.Length;

                nm = varuse.Match(currentline);
                nm2 = varuse2.Match(currentline);
                nm3 = stringvaruse.Match(currentline);

                or = "";
                and = "";
                then = "";
                sdo = "";

                if (nm.Index + 2 < currentline.Length)
                    or = currentline[clength - 2].ToString() + currentline[clength - 1].ToString();
                if (nm.Index + 2 < currentline.Length)
                    sdo = currentline[clength - 2].ToString() + currentline[clength - 1].ToString();
                if (nm2.Index + 3 < currentline.Length)
                    and = currentline[clength - 3].ToString() + currentline[clength - 2].ToString() + currentline[clength - 1].ToString();
                if (nm3.Index + 4 < currentline.Length)
                    then = currentline[clength - 4].ToString() + currentline[clength - 3].ToString() + currentline[clength - 2].ToString() + currentline[clength - 1].ToString();

                if (nm.Success || nm2.Success || nm3.Success)
                {
                    int index = 10000;
                    if (nm.Success)
                        if (nm.Index < index)
                            index = nm.Index;
                    if (nm2.Success)
                        if (nm2.Index < index)
                            index = nm2.Index;
                    if (nm3.Success)
                        if (nm3.Index < index)
                        {
                            index = nm3.Index;
                            index++;
                        }
                    if (sdo != "do" && or != "or " && and != "and " && then != "then")
                    {
                        // split the line
                        // set the current line to the first half of split
                        // insert the second half at the next index
                        // this causes recursion
                        string beginline = currentline.Remove(index + 1, currentline.Length - index - 1);
                        string nextline = currentline.Remove(0, index + 1);
                        nfilelines[i] = beginline;
                        nfilelines.Insert(i + 1, nextline);
                    }
                }
            }

            // format the tab spacing
            int tabindex = 0;

            Dictionary<int, string> tabs = new Dictionary<int, string>();
            tabs.Add(0,  "");
            tabs.Add(1,  "\t");
            tabs.Add(2,  "\t\t");
            tabs.Add(3,  "\t\t\t");
            tabs.Add(4,  "\t\t\t\t");
            tabs.Add(5,  "\t\t\t\t\t");
            tabs.Add(6,  "\t\t\t\t\t\t");
            tabs.Add(7,  "\t\t\t\t\t\t\t");
            tabs.Add(8,  "\t\t\t\t\t\t\t\t");
            tabs.Add(9,  "\t\t\t\t\t\t\t\t\t");
            tabs.Add(10, "\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(11, "\t\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(12, "\t\t\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(13, "\t\t\t\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(14, "\t\t\t\t\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(15, "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(16, "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t");

            for (int i = 0; i < nfilelines.Count; i++)
            {

                currentline = nfilelines[i].Trim();
                if (currentline.IndexOf("end") == 0)
                {
                        tabindex--;
                }
                else if (currentline.IndexOf("else") == 0)
                    tabindex--;
                else if (currentline.IndexOf("elseif") == 0)
                    tabindex--;

                // more array code
                if (currentline.IndexOf("},") == 0)
                {
                    string nextline = currentline.Remove(0, 2);
                    nfilelines[i] = "},";
                    nfilelines.Insert(i + 1, nextline);
                    tabindex--;
                }
                else if (currentline.IndexOf('}') == 0)
                {
                    string nextline = currentline.Remove(0, 1);
                    nfilelines[i] = "}";
                    nfilelines.Insert(i + 1, nextline);
                    tabindex--;
                }
                

                if (tabindex < 0)
                    tabindex = 0;

                currentline = tabs[tabindex] + nfilelines[i];
                nfilelines[i] = currentline;

                currentline = currentline.Trim();

                if (currentline.IndexOf("if") == 0)
                    tabindex++;
                else if (currentline.IndexOf("elseif") == 0)
                    tabindex++;
                else if (currentline.IndexOf("else") == 0)
                    tabindex++;
                else if (currentline.IndexOf("function ") == 0)
                    tabindex++;
                else if (currentline.IndexOf("function(") == 0)
                    tabindex++;
                else if (currentline.IndexOf("while") == 0)
                    tabindex++;
                else if (currentline.IndexOf("for") == 0)
                    tabindex++;
                else if (currentline.IndexOf("local function")==0)
                    tabindex++;

                // array code
                if (!currentline.Contains('{') && currentline.IndexOf('}') > 0)
                {
                    int bindex = currentline.IndexOf('}');
                    if (bindex + 1 < currentline.Length && currentline[bindex + 1] == ',')
                    {
                        string newcurrentline = currentline.Remove(bindex, currentline.Length - bindex);
                        nfilelines[i] = tabs[tabindex] + newcurrentline;
                        nfilelines.Insert(i + 1, currentline.Remove(0, bindex));
                    }
                    else
                    {
                        string newcurrentline = currentline.Remove(bindex, currentline.Length - bindex);
                        nfilelines[i] = tabs[tabindex] + newcurrentline;
                        nfilelines.Insert(i + 1, currentline.Remove(0, bindex));
                    }
                }

                if (currentline.IndexOf('{') == 0)
                {
                    string nextline = currentline.Remove(0, 1);
                    nfilelines[i] = tabs[tabindex] + "{";
                    nfilelines.Insert(i + 1, nextline);
                    tabindex++;
                }

                // super hack OMGGG - what is even happening 
                if (currentline.Length > 2 && i + 2 < nfilelines.Count && currentline.Last() == '\'' && currentline[currentline.Length - 2] == '{')
                {
                    currentline = tabs[tabindex] + currentline.Remove(currentline.Length - 2, 2);
                    nfilelines[i] = currentline;
                    nfilelines.Insert(i + 1, "{");
                    nfilelines[i + 2] = "\'" + nfilelines[i + 2];
                }
                
                // adds a blank line if function/local variable is defined, by checking next line
                if (tabindex == 0 && i+1 < nfilelines.Count && !currentline.Contains("local ") && (nfilelines[i+1].IndexOf("function ") == 0 || nfilelines[i+1].IndexOf("local ") == 0))
                {
                    nfilelines.Insert(i + 1, "");
                    i++;
                }
            }

            File.WriteAllLines(output, nfilelines.ToArray());
        }

        /// <summary>
        /// Formats/unminfiy the given lua file
        /// </summary>
        /// <param name="path">input file</param>
        /// <param name="output">save path</param>
        public static List<string> formatfile(string[] input, ref List<LuaDefination> defs)
        {
            string[] flines = input;
            List<string> nfilelines = new List<string>(flines.Length);

            int lineindex = 0;
          
            string currentline = "";

            Match m = Match.Empty;
            LuaDefination d1 = new LuaDefination();
            LuaDefination d2 = new LuaDefination();

            for (int i = 0; i < flines.Length; i++)
            {
                currentline = flines[i];
                lineindex = i;

                string orgst = currentline;
                string searchtext = currentline;

                int m1index = 0;
                m = Match.Empty;
                bool set = false;
                int m1length = 0;
                d1 = null;
                d2 = null;

                while (true)
                {
                    m1length = 0;

                    // match searchtext with regexs in our defs, keep track of the lowest match index
                    foreach (var c in defs)
                    {
                        Match m2 = c.Match(searchtext);

                        if (m2.Success && (m2.Index <= m.Index || !set))
                        {
                            if (searchtext.Contains('\''))
                            {
                                int mindex = m2.Index;

                                bool good1 = false;
                                bool good2 = false;


                                //search if we are in a string backwards
                                for (int check = mindex; check > -1; check--)
                                {
                                    if (searchtext[check] == '\'')
                                    {
                                        break;
                                    }
                                    else if (searchtext[check] == '=' || searchtext[check] == '(' || searchtext[check] == '{' || searchtext[check] == ',' || searchtext[check] == ')' || searchtext[check] == '}')
                                    {
                                        good1 = true;
                                        break;
                                    }
                                }

                                //search if we are in a string forewards
                                if (!good1)
                                {
                                    for (int check = mindex; check < searchtext.Length; check++)
                                    {
                                        if (searchtext[check] == '\'')
                                        {
                                            break;
                                        }
                                        else if (searchtext[check] == '=' || searchtext[check] == ')' || searchtext[check] == '}' || searchtext[check] == ',' || searchtext[check] == '(' || searchtext[check] == '{')
                                        {
                                            good2 = true;
                                            break;
                                        }
                                    }
                                }

                                if ((good1 && good2) || (good1 && !good2) || (!good1 && good2))
                                {
                                    set = true;
                                    m = m2;
                                    d1 = c;
                                }
                            }
                            else
                            {
                                set = true;
                                m = m2;
                                d1 = c;
                            }
                        }
                    }


                    if (m.Success)
                    {
                        m1length = m.Length;
                        m1index = m.Index;
                        set = false;
                        // total hack, didn't want to spend the time
                        if (m1index < 1)
                            // remove what was matched in m
                            searchtext = searchtext.Remove(m.Index + d1.indexoffset, m.Value.Length -d1.indexoffset);

                        m = Match.Empty;
                        // search again to find the next match in search text (remember m's match was removed)
                        foreach (var c in defs)
                        {
                            Match m2 = c.Match(searchtext);

                            if (m2.Success && (m2.Index <= m.Index || !set))
                            {
                                if (searchtext.Contains('\''))
                                {
                                    int mindex = m2.Index;

                                    bool good1 = false;
                                    bool good2 = false;

                                    for (int check = mindex; check > -1; check--)
                                    {
                                        if (searchtext[check] == '\'')
                                        {
                                            break;
                                        }
                                        else if (searchtext[check] == '=' || searchtext[check] == '=' || searchtext[check] == '(' || searchtext[check] == '{' || searchtext[check] == ',' || searchtext[check] == ')' || searchtext[check] == '}')
                                        {
                                            good1 = true;
                                            break;
                                        }
                                    }

                                    if (!good1)
                                    {
                                        for (int check = mindex; check < searchtext.Length; check++)
                                        {
                                            if (searchtext[check] == '\'')
                                            {
                                                break;
                                            }
                                            else if (searchtext[check] == ')' || searchtext[check] == '}' || searchtext[check] == ',' || searchtext[check] == '(' || searchtext[check] == '{')
                                            {
                                                good2 = true;
                                                break;
                                            }
                                        }
                                    }

                                    if ((good1 && good2) || (good1 && !good2) || (!good1 && good2))
                                    {
                                        set = true;
                                        m = m2;
                                        d2 = c;
                                    }
                                }
                                else
                                {
                                    set = true;
                                    m = m2;
                                    d2 = c;
                                }
                            }
                        }

                        // successfully found a second match and our original search matched at index of 0
                        if (m.Success && m1index < 1)
                        {
                            //remove text from 0 index to our second match + our offset
                            //this removes the current line we just found from search text
                            searchtext = searchtext.Remove(0, m.Index+d2.indexoffset);
                            //extracts the line of lua using our 2 matches
                            string line = orgst.Remove(m.Index + m1length + d1.indexoffset + d2.indexoffset, orgst.Length - m.Index - m1length - d1.indexoffset - d2.indexoffset);
                            // our list of lua lines
                            nfilelines.Add(line);
                            // remove our line  from orgst
                            orgst = orgst.Remove(0, m.Index + m1length + d1.indexoffset + d2.indexoffset);
                            //clean up
                            set = false;
                            m = Match.Empty;
                        }
                        /* Hack solutution for match 1 find a match further that the index of 1
                         * this causes problems in how we split the line
                         * so I cheated and put this in
                         */
                        else if (m1index > 1 && m1length > 0)
                        {
                            searchtext = searchtext.Remove(0, m1index+d1.indexoffset);
                            // remove from match 1 index to end of orgst
                            // basically get our lua line
                            string line = orgst.Remove(m1index+d1.indexoffset, orgst.Length - m1index-d1.indexoffset);
                            nfilelines.Add(line);
                            // remove our lua line from orgst
                            orgst = orgst.Remove(0, m1index+d1.indexoffset);
                            set = false;
                            m = Match.Empty;
                        }
                        // only match 1 found something not 2 therefore we have a valid line, add it to list of lua lines
                        else
                        {
                            m = Match.Empty;
                            nfilelines.Add(orgst);
                            set = false;
                            break;
                        }
                    }
                    else
                    {
                        m = Match.Empty;
                        nfilelines.Add(orgst);
                        set = false;
                        break;
                    }
                    set = false;
                    m = Match.Empty;
                }
            }

            /* stage 2 find variables accessed on a line
             * 
             * ex
             * 
             * local t = getonions()tomato.water=0
             * 
             * these should be on seperat lines
             * 
             * local t = getonions()
             * tomato.water=0
             * 
             * 
            */

            Regex varuse = new Regex(@"\)\w");   // finds ) any letter
            Regex varuse2 = new Regex(@"\}\w");  // finds } any letter
            Regex stringvaruse = new Regex(@"[^=(,\[]'\w(?<!.*require.*)"); // matches any letter other than =(,[ then ' then any letter, if success then tries to match require , if require matches success=false
            Match nm  = Match.Empty;
            Match nm2 = Match.Empty;
            Match nm3 = Match.Empty;

            string or = "";
            string and = "";
            string then = "";
            string sdo = "";

            for (int i = 0; i < nfilelines.Count; i++)
            {
                currentline = nfilelines[i];
                int clength = currentline.Length;

                nm = varuse.Match(currentline);
                nm2 = varuse2.Match(currentline);
                nm3 = stringvaruse.Match(currentline);

                or = "";
                and = "";
                then = "";
                sdo = "";

                if (nm.Index + 2 < currentline.Length)
                    or = currentline[clength - 2].ToString() + currentline[clength - 1].ToString();
                if (nm.Index + 2 < currentline.Length)
                    sdo = currentline[clength - 2].ToString() + currentline[clength - 1].ToString();
                if (nm2.Index + 3 < currentline.Length)
                    and = currentline[clength - 3].ToString() + currentline[clength - 2].ToString() + currentline[clength - 1].ToString();
                if (nm3.Index + 4 < currentline.Length)
                    then = currentline[clength - 4].ToString() + currentline[clength - 3].ToString() + currentline[clength - 2].ToString() + currentline[clength - 1].ToString();

                if (nm.Success || nm2.Success || nm3.Success)
                {
                    int index = 10000;
                    if (nm.Success)
                        if (nm.Index < index)
                            index = nm.Index;
                    if (nm2.Success)
                        if (nm2.Index < index)
                            index = nm2.Index;
                    if (nm3.Success)
                        if (nm3.Index < index)
                        {
                            index = nm3.Index;
                            index++;
                        }
                    if (sdo != "do" && or != "or " && and != "and " && then != "then")
                    {
                        // split the line
                        // set the current line to the first half of split
                        // insert the second half at the next index
                        // this causes recursion
                        string beginline = currentline.Remove(index + 1, currentline.Length - index - 1);
                        string nextline = currentline.Remove(0, index + 1);
                        nfilelines[i] = beginline;
                        nfilelines.Insert(i + 1, nextline);
                    }
                }
            }

            // format the tab spacing
            int tabindex = 0;

            Dictionary<int, string> tabs = new Dictionary<int, string>();
            tabs.Add(0,  "");
            tabs.Add(1,  "\t");
            tabs.Add(2,  "\t\t");
            tabs.Add(3,  "\t\t\t");
            tabs.Add(4,  "\t\t\t\t");
            tabs.Add(5,  "\t\t\t\t\t");
            tabs.Add(6,  "\t\t\t\t\t\t");
            tabs.Add(7,  "\t\t\t\t\t\t\t");
            tabs.Add(8,  "\t\t\t\t\t\t\t\t");
            tabs.Add(9,  "\t\t\t\t\t\t\t\t\t");
            tabs.Add(10, "\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(11, "\t\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(12, "\t\t\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(13, "\t\t\t\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(14, "\t\t\t\t\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(15, "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t");
            tabs.Add(16, "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t");

            for (int i = 0; i < nfilelines.Count; i++)
            {

                currentline = nfilelines[i].Trim();
                if (currentline.IndexOf("end") == 0)
                {
                        tabindex--;
                }
                else if (currentline.IndexOf("else") == 0)
                    tabindex--;
                else if (currentline.IndexOf("elseif") == 0)
                    tabindex--;

                // more array code
                if (currentline.IndexOf("},") == 0)
                {
                    string nextline = currentline.Remove(0, 2);
                    nfilelines[i] = "},";
                    nfilelines.Insert(i + 1, nextline);
                    tabindex--;
                }
                else if (currentline.IndexOf('}') == 0)
                {
                    string nextline = currentline.Remove(0, 1);
                    nfilelines[i] = "}";
                    nfilelines.Insert(i + 1, nextline);
                    tabindex--;
                }
                

                if (tabindex < 0)
                    tabindex = 0;

                currentline = tabs[tabindex] + nfilelines[i];
                nfilelines[i] = currentline;

                currentline = currentline.Trim();

                if (currentline.IndexOf("if") == 0)
                    tabindex++;
                else if (currentline.IndexOf("elseif") == 0)
                    tabindex++;
                else if (currentline.IndexOf("else") == 0)
                    tabindex++;
                else if (currentline.IndexOf("function ") == 0)
                    tabindex++;
                else if (currentline.IndexOf("function(") == 0)
                    tabindex++;
                else if (currentline.IndexOf("while") == 0)
                    tabindex++;
                else if (currentline.IndexOf("for") == 0)
                    tabindex++;
                else if (currentline.IndexOf("local function")==0)
                    tabindex++;

                // array code
                if (!currentline.Contains('{') && currentline.IndexOf('}') > 0)
                {
                    int bindex = currentline.IndexOf('}');
                    if (bindex + 1 < currentline.Length && currentline[bindex + 1] == ',')
                    {
                        string newcurrentline = currentline.Remove(bindex, currentline.Length - bindex);
                        nfilelines[i] = tabs[tabindex] + newcurrentline;
                        nfilelines.Insert(i + 1, currentline.Remove(0, bindex));
                    }
                    else
                    {
                        string newcurrentline = currentline.Remove(bindex, currentline.Length - bindex);
                        nfilelines[i] = tabs[tabindex] + newcurrentline;
                        nfilelines.Insert(i + 1, currentline.Remove(0, bindex));
                    }
                }

                if (currentline.IndexOf('{') == 0)
                {
                    string nextline = currentline.Remove(0, 1);
                    nfilelines[i] = tabs[tabindex] + "{";
                    nfilelines.Insert(i + 1, nextline);
                    tabindex++;
                }

                // super hack OMGGG - what is even happening 
                if (currentline.Length > 2 && i + 2 < nfilelines.Count && currentline.Last() == '\'' && currentline[currentline.Length - 2] == '{')
                {
                    currentline = tabs[tabindex] + currentline.Remove(currentline.Length - 2, 2);
                    nfilelines[i] = currentline;
                    nfilelines.Insert(i + 1, "{");
                    nfilelines[i + 2] = "\'" + nfilelines[i + 2];
                }
                
                // adds a blank line if function/local variable is defined, by checking next line
                if (tabindex == 0 && i+1 < nfilelines.Count && !currentline.Contains("local ") && (nfilelines[i+1].IndexOf("function ") == 0 || nfilelines[i+1].IndexOf("local ") == 0))
                {
                    nfilelines.Insert(i + 1, "");
                    i++;
                }
            }

            return nfilelines;
        }
    }



    public class LuaDefination
    {
        public int indexoffset { get; set; }
        public Regex regex;

        public LuaDefination()
        {

        }

        public Match Match(string text)
        {
            return regex.Match(text);
        }
    }
}
