using XLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal static class XLuaVM {
    private static LuaEnv instance;
    public static LuaEnv Instance {
        get {
            if (instance == null) {
                instance = new LuaEnv();
                instance.DoString("Dialog={Ctx={}}");
            }
            return instance;
        }
    }
    public static object GetInDialog(this LuaEnv instance, string varName) {
        return instance.Global.Get<object>("Dialog." + varName);
    }
    public static void SetInDialog<T>(this LuaEnv instance, string varName, T val) {
        instance.Global.Set("Dialog." + varName, val);
    }
    public static object Get(this LuaEnv instance, string varName) {
        return instance.Global.Get<object>(varName);
    }
    public static void Set<T>(this LuaEnv instance, string varName, T val) {
        instance.Global.Set(varName, val);
    }
}
