using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Interface for standard bot runs and rushes
public interface IBot
{
    void ResetVars();
    void RunScript();
    bool ScriptDone { get; set; }

}
