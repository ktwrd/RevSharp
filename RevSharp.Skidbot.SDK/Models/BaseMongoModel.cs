using System.ComponentModel;
using MongoDB.Bson;

namespace RevSharp.Skidbot.Models;

public class BaseMongoModel
{
    [Browsable(false)]
    public ObjectId _id { get; set; }
}