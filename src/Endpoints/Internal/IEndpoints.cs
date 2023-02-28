public interface IEndpoints{

    public static abstract void AddEndpoints(IEndpointRouteBuilder app);
    public static abstract void AddServices(IServiceCollection services,IConfiguration configuration);
}