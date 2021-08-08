using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
[LuaApi(luaName = "Core",description = "Core functionalities for Unity Party Lua scripting.")]
public class LuaScripting : LuaAPIBase
{

    public LuaScripting() : base("Core")
    {
        
    }

    protected override void InitialiseAPITable()
    {
        m_ApiTable["GetCurrentBeat"] = (System.Func<int>) GetCurrentBeat;
    }
    [LuaApiFunction(
        name = "GetCurrentBeat",
        description = "Returns the current beat for the currently playing song."
    )]
    private int GetCurrentBeat()
    {
        return Song.instance.currentBeat;
    }
    
    
}
