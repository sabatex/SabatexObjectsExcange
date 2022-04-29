using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using WebApi1C8Exchange.Data;
using WebApi1C8Exchange.Models;

namespace WebApi1C8Exchange.Controllers;

public static class ExcangeExtensions
{
    /// <summary>
    /// Повертає нод для комбінації пароль + нод
    /// </summary>
    /// <param name="_context"></param>
    /// <param name="senderNode"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<ClientNode> GetSenderAsync(this ApplicationDbContext _context, [NotNull]string senderNode, string password)
    {
        // find source node
        var sender = await _context.ClientNodes.FindAsync(senderNode);
        if (sender == null)
            throw new ArgumentException("Node {0} not exist!", senderNode);
            
        // check password
        if (sender.Password != password)
            throw new ArgumentException("Password is fail for node {0}", senderNode);
        return sender;
    }
    public static async Task<ClientNode?> GetDestinationNode(this ApplicationDbContext _context, [NotNull] string destinationNode)
    {
        // find source node
        var result = await _context.ClientNodes.FindAsync(destinationNode);
        return result;
    }

    public static async Task<(ClientNode sender,ClientNode destination)> GetSendNodesAsync(this ApplicationDbContext _context,
                                                                                            [NotNull] string senderNode,
                                                                                            [NotNull] string destinationNode,
                                                                                            [NotNull] string apiKey)
    {
        // find sender node
        var sender = await _context.ClientNodes.FindAsync(senderNode);
        if (sender == null)
            throw new ArgumentException("Node {0} not exist!", senderNode);

        // check password
        if (sender.Password != apiKey)
            throw new ArgumentException("Password is fail for node {0}", senderNode);

        // find destination node
        var destination = await _context.ClientNodes.FindAsync(destinationNode);
        if (destination == null)
            throw new ArgumentException("Node {0} not exist!", destinationNode);
        return (sender,destination);
    }

    public static async Task<ClientNode> GetNodeAsync(this ApplicationDbContext _context, [NotNull] string node)
    {
        var result = await _context.ClientNodes.FindAsync(node);
        if (result == null)
            throw new ArgumentException("Node {0} not exist!", node);

        return result;
    }
    public static async Task<ClientNode> GetSecureNodeAsync(this ApplicationDbContext _context, [NotNull] string node, string apiKey)
    {
        var result = await GetNodeAsync(_context, node);
        // check password
        if (result.Password != apiKey)
            throw new ArgumentException("Password is fail for node {0}", node);
        return result;
    }


    public static async Task<ClientObjectType> GetClientObjectTypeAsync(this ApplicationDbContext _context, ClientNode node,[NotNull] string objectType)
    {
        var result = await _context.ClientObjectTypes.SingleOrDefaultAsync(t => t.Node.Id == node.Id && t.Name == objectType);
        if (result != null)
            return result;
        result = new ClientObjectType { Name = objectType,Node = node };
        await _context.AddAsync(result);
        await _context.SaveChangesAsync();
        return result;
    }

}
