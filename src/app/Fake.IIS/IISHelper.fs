﻿[<AutoOpen>]
module Fake.IISHelper

    open Microsoft.Web.Administration
    open Fake.PermissionsHelper

    let private bindApplicationPool (appPool : ApplicationPool) (app : Application) =
        app.ApplicationPoolName <- appPool.Name

    let Site (name : string) (protocol : string) (binding : string) (physicalPath : string) (appPool : string) (mgr : ServerManager) = 
        let site = mgr.Sites.Add(name, protocol, binding, physicalPath)
        site.ApplicationDefaults.ApplicationPoolName <- appPool
        site

    let ApplicationPool (name : string) (mgr : ServerManager) = 
        mgr.ApplicationPools.Add(name)

    let Application (virtualPath : string) (physicalPath : string) (site : Site) (mgr : ServerManager) =
        site.Applications.Add(virtualPath, physicalPath)

    let commit (mgr : ServerManager) = mgr.CommitChanges();

    let IIS (site : ServerManager -> Site) 
            (appPool : ServerManager -> ApplicationPool) 
            (app : (Site -> ServerManager -> Application) option) =
        use mgr = new ServerManager()
        requiresAdmin (fun _ -> 
                            match app with
                            | Some(app) -> bindApplicationPool (appPool mgr) (app (site mgr) mgr); 
                            | None -> bindApplicationPool (appPool mgr) (site mgr).Applications.[0]
                            commit mgr
                      )

    let deleteSite (name : string) = 
        use mgr = new ServerManager()
        let site = mgr.Sites.[name]
        if (site <> null) then
            site.Delete()
            commit mgr 

    let deleteApp (name : string) (site : Site) = 
        use mgr = new ServerManager()
        let app = site.Applications.[name]
        if (app <> null) then
            app.Delete()
            commit mgr

    let deleteApplicationPool (name : string) = 
        use mgr = new ServerManager()
        let appPool = mgr.ApplicationPools.[name]
        if (appPool <> null) then
            appPool.Delete()
            commit mgr