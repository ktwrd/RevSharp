using System.ComponentModel;
using MongoDB.Bson;

namespace RevSharp.ReBot.Models;

public class BaseMongoModel
{
    [Browsable(false)]
    public ObjectId _id { get; set; }
}