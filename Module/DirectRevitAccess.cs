//
// (C) Copyright 2007-2016 by Autodesk, Inc. All rights reserved.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM 'AS IS' AND WITH ALL ITS FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.


/// <summary>
/// </summary>

using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.REX.Framework;
using REX.Common;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace REX.AC_Extension
{

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class DirectRevitExternalApplication : IExternalApplication
    {
        private UIControlledApplication app = null;

        public Result OnShutdown(UIControlledApplication application)
        {
            app = null;
            return Proxy.OnShutdown(application);
        }

        public Result OnStartup(UIControlledApplication application)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            if (currentDomain != null)
                currentDomain.AssemblyResolve += new ResolveEventHandler(currentDomain_AssemblyResolve);

            if (application.IsLateAddinLoading)
            {
                return Proxy.OnStartup(application);
            }
            else
            {
                app = application;
                application.ControlledApplication.ApplicationInitialized += new EventHandler<Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs>(ApplicationInitialized);
                return Result.Succeeded;
            }
        }

        private void ApplicationInitialized(object sender, Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
        {
            if (app != null)
                Proxy.OnStartup(app);
        }

        private bool isFirstRun = true;
        System.Reflection.Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (isFirstRun)
            {
                if (REXEnvironment.Current == null)
                {
                    Autodesk.REX.Framework.REXEnvironment Env = new Autodesk.REX.Framework.REXEnvironment();
                }

                Autodesk.REX.Framework.REXAssemblies.AssembliesPaths.Add(REXEnvironment.Current.GetPath(REXEnvironment.PathType.Engine, "Revit"));
                Autodesk.REX.Framework.REXAssemblies.Resolve(sender, args, GetType().Assembly);

                isFirstRun = false;
            }

            return Autodesk.REX.Framework.REXAssemblies.Resolve(sender, args, GetType().Assembly);
        }

        private static class Proxy
        {
            private static ExtensionDirectRevitApplication extensionDirectRevitApplication = null;

            public static Result OnShutdown(UIControlledApplication application)
            {
                return extensionDirectRevitApplication.OnShutdown(application);
            }

            public static Result OnStartup(UIControlledApplication application)
            {
                if (extensionDirectRevitApplication == null)
                    extensionDirectRevitApplication = new ExtensionDirectRevitApplication();

                return extensionDirectRevitApplication.OnStartup(application);
            }
        }
    }

    /// <summary>
    /// The class enables direct connection between Revit and extension.  
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class DirectRevitAccess : IExternalCommand
    {
        /// <summary>
        /// Executes the extension.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        /// <param name="message">The message.</param>
        /// <param name="elements">The elements.</param>
        /// <returns>Returns execution result.</returns>
        public Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            if (currentDomain != null)
                currentDomain.AssemblyResolve += new ResolveEventHandler(currentDomain_AssemblyResolve);

            return Proxy.Execute(commandData, ref message, elements);
        }

        System.Reflection.Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Autodesk.REX.Framework.REXConfiguration.Initialize(System.Reflection.Assembly.GetExecutingAssembly());
            return Autodesk.REX.Framework.REXAssemblies.Resolve(sender, args, "2016", System.Reflection.Assembly.GetExecutingAssembly());
        }

        private static class Proxy
        {
            public static Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                ExtensionDirectRevitAccess extensionDirectRevitAccess = new ExtensionDirectRevitAccess();
                return extensionDirectRevitAccess.Execute(commandData, ref message, elements);
            }
        }
    }


    /// <summary>
    /// The class enables direct connection between Revit and extension.  
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ExtensionDirectRevitAccess : REXDirectRevitAccess, IExternalCommand
    {
        /// <summary>
        /// Executes the extension.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        /// <param name="message">The message.</param>
        /// <param name="elements">The elements.</param>
        /// <returns>Returns execution result.</returns>
        public Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                AppRef = new Application();

                REXContext context = new REXContext();
                context.Control.Mode = REXMode.Dialog;
                context.Control.ControlMode = REXControlMode.ModalDialog;
                PresetContext(ref commandData, ref message, ref elements, ref context);

                REXEnvironment Env = new REXEnvironment(REXConst.ENG_DedicatedVersionName);
                if (Env.LoadEngine(ref context))
                {
                    if (AppRef.Create(ref context))
                        AppRef.Show();
                }
                if (context.Extension.Result == REXResultType.None || context.Extension.Result == REXResultType.Succeeded)
                    return Result.Succeeded;
                else if (context.Extension.Result == REXResultType.Cancelled)
                    return Result.Cancelled;
                else
                    return Result.Failed;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private Application AppRef;
    }


    /// 
    /// The class enables direct connection between Revit and extension.
    /// 
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ExtensionDirectRevitApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return REX.AREXRevitStart.AREXRevitStartManager.OnShutdown(application);
        }

        public Result OnStartup(UIControlledApplication application)
        {
            Guid guid = new Guid("A0AED6F5-08F8-4B59-926C-A99EA44994E8");

            REX.AREXRevitStart.AREXRevitStartManager.RegisterAddInGuid(REXConst.REX_RevitSubNameOneBox, guid);
            REX.AREXRevitStart.AREXRevitStartManager.RegisterAddInGuid(REXConst.REX_RevitSubNameStructure, guid);


            REX.AREXRevitStart.AREXRevitStartManager.RegisterAddInGuid(REXConst.REX_RevitSubNameArchitecture, guid);


            REX.AREXRevitStart.AREXRevitStartManager.RegisterAddInGuid(REXConst.REX_RevitSubNameMEP, guid);

            REX.AREXRevitStart.AREXRevitStartManager.OnStartup(application);

            Common.Start.ModuleData data = new Common.Start.ModuleData();
            data.Name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            data.Path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            // display Modify, Delete and Preferences buttons in Extensions ribbon
            data.ControlPanel = false;

            // register for modification mechanism
            Autodesk.REX.Framework.REXEnvironment.Current.RegisterInternalModuleName(data.Name);

            if (REX.AREXRevitStart.AREXRevitStartManager.AddModule(data))
                return Result.Succeeded;

            return Result.Failed;
        }
    }

}
