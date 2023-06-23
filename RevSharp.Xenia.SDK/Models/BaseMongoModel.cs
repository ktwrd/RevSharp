using System.ComponentModel;
using MongoDB.Bson;

namespace RevSharp.Xenia.Models;

public class BaseMongoModel
{
    [Browsable(false)]
    public ObjectId _id { get; set; }
}