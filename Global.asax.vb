Public Class MvcApplication
    Inherits HttpApplication

    Sub Application_Start()
        AreaRegistration.RegisterAllAreas()
        RegisterGlobalFilters(GlobalFilters.Filters)
        RegisterRoutes(RouteTable.Routes)
    End Sub
End Class
