using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

public class GameConsole {
    TMP_Text tmp_txt;
    int numOfChars = 0;
    Queue<string> messages = new();
    public GameConsole(TMP_Text tmp_text) {
        tmp_txt = tmp_text;
    }
    public void Log(string msg) {
        messages.Enqueue(msg);
        numOfChars += msg.Length;
        while (IsOverflow())
            PopMessage();
        tmp_txt.text = string.Join('\n', messages);
    }
    void PopMessage() {
        numOfChars -= messages.Dequeue().Length;
    }
    bool IsOverflow() {
        return numOfChars > 6000 || messages.Count > 300;
    }
}
