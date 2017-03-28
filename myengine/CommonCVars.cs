using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	public class CommonCVars
	{
		public readonly MyDebug Debug;
		public CommonCVars(MyDebug debug)
		{
			this.Debug = debug;
		}
		public CVar GetCVar([CallerMemberName] string name = null)
		{
			return Debug.GetCVar(name);
		}
	}
	/*
	public static class CommonCVarsExtensions
	{
		public static CVar ShowDebugForm(this CommonCVars c) => c.GetCVar();
		public static CVar PauseRenderPrepare(this CommonCVars c) => c.GetCVar();
		public static CVar ReloadAllShaders(this CommonCVars c) => c.GetCVar();
		public static CVar ShadowsDisabled(this CommonCVars c) => c.GetCVar();
		public static CVar DrawDebugBounds(this CommonCVars c) => c.GetCVar();
		public static CVar EnablePostProcessEffects(this CommonCVars c) => c.GetCVar();
		public static CVar DebugDrawGBufferContents(this CommonCVars c) => c.GetCVar();
		public static CVar DebugDrawNormalBufferContents(this CommonCVars c) => c.GetCVar();
		public static CVar DebugRenderWithLines(this CommonCVars c) => c.GetCVar();
		public static CVar Fullscreen(this CommonCVars c) => c.GetCVar();
		public static CVar VSync(this CommonCVars c) => c.GetCVar();		
	}
	*/
}
