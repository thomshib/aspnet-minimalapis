

using System.Reflection;

public static class EndpointExtensions{


 

     private static IEnumerable<TypeInfo> GetTypeNamesFromContainingAssesmbly(Type typeMarker)
    {

        //look for all types that implement IEndpoints like LibraryEndpoint class

        return typeMarker.Assembly.DefinedTypes
                .Where(x => !x.IsAbstract && !x.IsInterface &&
                typeof(IEndpoints).IsAssignableFrom(x));
    }

    public static void AddServices<TMarker>(this IServiceCollection services,
    IConfiguration configuration){

        AddServices(services, typeof(TMarker), configuration);
        
    }
    public static void AddServices(this IServiceCollection services,
    Type typeMarker, IConfiguration configuration)
    {
        IEnumerable<TypeInfo> endpointTypes = GetTypeNamesFromContainingAssesmbly(typeMarker);


        foreach (var endpointType in endpointTypes)
        {

            endpointType.GetMethod(nameof(IEndpoints.AddServices))!
            .Invoke(null, new object[] { services, configuration });

        }
    }

   

    public static void UseEndpoints<TMarker>(this IApplicationBuilder app){
        UseEndpoints(app, typeof(TMarker));
    }

    public static void UseEndpoints(this IApplicationBuilder app, Type typeMarker){

        IEnumerable<TypeInfo> endpointTypes = GetTypeNamesFromContainingAssesmbly(typeMarker);


        foreach (var endpointType in endpointTypes)
        {

            endpointType.GetMethod(nameof(IEndpoints.AddEndpoints))!
            .Invoke(null, new object[] { app});

        }

    }


}