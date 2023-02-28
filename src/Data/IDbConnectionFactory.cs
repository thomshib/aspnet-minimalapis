
using System.Data;
using Dapper;

public interface IDbConnectionFactory{

    Task<IDbConnection> CreateConnectionAsync();
}