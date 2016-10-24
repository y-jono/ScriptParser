using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static String text = @"
begin setup
    hoge
end

begin main
    fuga
end

begin teardown
    hoge
end
";
        class Steps
        {
            IEnumerable<Step> steps = new List<Step>();
        }

        class Blocks : IEnumerable<Block>
        {
            IEnumerable<Block> blocks = new List<Block>();

            public IEnumerator<Block> GetEnumerator()
            {
                return blocks.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal void Add(Block block)
            {
                //throw new NotImplementedException();
            }
        }
        class Block
        {
            public Block(string name)
            {
                this.Name = name;
            }

            public string Name { get; private set; } = String.Empty;

            internal void Add(string line)
            {
                //throw new NotImplementedException();
            }
        }
        class Step { }
        IEnumerable<Block> functions = new List<Block>();

        static void Main(string[] args)
        {
            var blocks = parseText(text);
            var stepsPerBlock = blocks.Select((b) => parseBlock(b));
        }

        private static IEnumerable<Step> parseBlock(Block block)
        {
            //throw new NotImplementedException();
            return new List<Step>();
        }

        enum BlockParseState
        {
            BlockOut,
            BlockIn
        }
        const string beginPattern = @"^\s*begin\s+(\w+)";
        const string endPattern = @"^\s*end";
        private static Blocks parseText(string text)
        {
            var state = BlockParseState.BlockOut;
            var sr = new StringReader(text);
            var line = String.Empty;
            var blocks = new Blocks();
            int nonamefunctionCounter = 0;
            var block = new Block($"nonamefunction{nonamefunctionCounter}");
            ++nonamefunctionCounter;
            var blockText = new List<string>();
            var funcName = String.Empty;
            while ((line = sr.ReadLine()) != null)
            {
                var beginmatch = Regex.Match(line, beginPattern);
                var endmatch = Regex.Match(line, endPattern);
                // begin文でもend文でもないとき
                if (!beginmatch.Success && !endmatch.Success)
                {
                    // 有効なlineのみをblockに追加する。
                    if (InvalidLine(line))
                    {
                        continue;
                    }

                    // 外側のスコープのとき
                    if (state == BlockParseState.BlockOut)
                    {
                        // beginやend文がない無名スコープ
                        // TODO: 無名スコープブロックとして、スクリプトを格納する
                        block.Add(line);
                    }
                    // 一つ内側のスコープのとき
                    else if (state == BlockParseState.BlockIn)
                    {
                        // TODO: funcNameに対応するブロックにスクリプトを格納する
                        block.Add(line);
                    }
                }
                // begin文で、一番そとのスコープのとき
                else if (beginmatch.Success && state == BlockParseState.BlockOut)
                {
                    state = BlockParseState.BlockIn;
                    funcName = beginmatch.Groups[1].Captures[0].Value;
                    blocks.Add(block);
                    block = new Block(funcName);
                }
                // end文で、１つ内側のスコープのとき
                else if (endmatch.Success && state == BlockParseState.BlockIn)
                {
                    state = BlockParseState.BlockOut;
                    funcName = String.Empty;
                    blocks.Add(block);
                    block = new Block($"nonamefunction{nonamefunctionCounter}");
                    ++nonamefunctionCounter;
                }
                // begin文で、１つ内側のスコープのとき
                else if (beginmatch.Success && state == BlockParseState.BlockIn)
                {
                    // 多重blockは未サポート
                    throw new InvalidOperationException("多重blockは未サポート");
                }
                // end文で、一番そとのスコープのとき
                else if (endmatch.Success && state == BlockParseState.BlockOut)
                {
                    // end文が１つ以上多い
                    throw new InvalidOperationException("end文が１つ以上多い");
                }
                // begin文もend文も含まれるとき
                else if (beginmatch.Success && endmatch.Success)
                {
                    throw new InvalidOperationException("文法エラー");
                }
            }
            return blocks;
        }

        private static bool InvalidLine(string line)
        {
            return line.Trim() == String.Empty;
        }
    }
}
