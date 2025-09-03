using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DialogNode {
    public string id;
    public string text;
    public DialogOption[] options;
    public override string ToString() {
        string str = $"Node:id={id}, text={text}, options:\n";
        foreach (DialogOption option in options) {
            str += option.ToString() ;
        }
        return str ;
    }
}
public class DialogOption {
    public string cond;
    public string text;
    public string next;
    public string trigger;
    public override string ToString() {
        return $"Option:cond={cond}, text={text}, next={next}, trigger={trigger}\n";
    }
}