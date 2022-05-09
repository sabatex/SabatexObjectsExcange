using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using WebApiDocumentsExchange.Data;
using WebApiDocumentsExchange.Models;

namespace WebApiDocumentsExchange.Extensions;

public static class ExcangeExtensions
{
    public static  TimeSpan TokenValid = TimeSpan.FromMinutes(15);
    public static async Task<int> GetNodeAsync(this ApplicationDbContext _context, [NotNull] string node)
    {
        var result = await _context.ClientNodes.SingleOrDefaultAsync(f=>f.Name==node);
        if (result == null)
            throw new ArgumentException("Node {0} not exist!", node);

        return result.Id;
    }
    public static async Task<int> GetSecureNodeAsync(this ApplicationDbContext _context, string apiToken)
    {
        var clientAutenficate = await _context.AutenficatedNodes.FindAsync(apiToken);
        if (clientAutenficate != null)
        {
            if ((DateTime.Now - clientAutenficate.DateStamp) < TokenValid)
            {
                var result = await _context.ClientNodes.FindAsync(clientAutenficate.NodeId);
                if (result != null)
                {
                    return result.Id;
                }
            }
 
        }
        throw new Exception("Access denied!!!");
     }

/// <summary>
/// 
/// </summary>
/// <param name="_context"></param>
/// <param name="node"></param>
/// <param name="password"></param>
/// <returns>Api token</returns>
/// <exception cref="Exception"></exception>
    public static async Task<string> Login(this ApplicationDbContext _context,string node, string password)
    {
        var clientNode = await _context.ClientNodes.SingleOrDefaultAsync(s => s.Name == node);
        if (clientNode != null)
        {
            if (clientNode.Password == password)
            {
                var result = await _context.AutenficatedNodes.SingleOrDefaultAsync(s => s.Node == clientNode); 
                if (result != null)
                {
                    _context.AutenficatedNodes.Remove(result); 
                }
                result = new AutenficatedNode { Node = clientNode,DateStamp = DateTime.Now, Id=Guid.NewGuid().ToString() };
                await _context.AutenficatedNodes.AddAsync(result);
                await _context.SaveChangesAsync();
                return result.Id.ToString();
            }
        }
        throw new Exception("The login or password incorect!");
    }

}
