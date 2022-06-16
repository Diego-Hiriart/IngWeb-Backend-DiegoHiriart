﻿using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Models;
using System.Linq;

namespace WebAPI_DiegoHiriart.Controllers
{
    [ApiController]
    [Route("api/stats")]
    public class StatisticsController : ControllerBase
    {
        [HttpGet("by-model/{id}")]
        public async Task<ActionResult<StatsInfo>> ModelStats(Int64 id)
        {
            List<Post> posts = new List<Post>();
            List<Issue> issues = new List<Issue>();
            List<Component> components = new List<Component>();
            StatsInfo statsInfo = new StatsInfo();

            string db = APIConfig.ConnectionString;
            string readPosts = "SELECT * FROM posts WHERE modelid = @0";
            string readIssues = "SELECT i.* FROM posts p " +
                                "INNER JOIN issues i on i.postid = p.postid " +
                                "WHERE p.modelid = @0";
            string readComponents = "SELECT * FROM components WHERE componentid IN (@0)";
            string componentIdList = "";

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readPosts;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var post = new Post();
                                    post.PostId = reader.GetInt64(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    post.UserId = reader.GetInt64(1);
                                    post.ModelId = reader.GetInt64(2);
                                    post.PostDate = reader.GetDateTime(3);
                                    post.Purchase = reader.GetDateTime(4);
                                    post.FirstIssues = reader[5] as DateTime?;
                                    post.Innoperative = reader[6] as DateTime?;
                                    post.Review = reader[7] as string;
                                    posts.Add(post);
                                }
                            }
                        }

                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readIssues;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var issue = new Issue();
                                    issue.IssueId = reader.GetInt64(0);//Get int from the first column
                                                                       //Use castings so that nulls get created if needed
                                    issue.PostId = reader.GetInt64(1);
                                    issue.ComponentId = reader.GetInt32(2);
                                    issue.IssueDate = reader.GetDateTime(3);
                                    issue.IsFixable = reader.GetBoolean(4);
                                    issue.Description = reader[5] as string;
                                    issues.Add(issue);
                                }
                            }
                        }
                        //Add component ids to be searched for
                        List<int> ids = new List<int>();//To check if ids have already been added to query
                        for (int i = 0; i < issues.Count; i++)//Get individual ids
                        {
                            if (!ids.Contains(issues[i].ComponentId)){//If the id has no tbeen added yet
                                ids.Add(issues[i].ComponentId);                               
                            }
                        }
                        foreach (int compId in ids)//Fill ids string
                        {
                            if (compId != ids[(ids.Count)-1])//Add with a coma (theres more after)
                            {
                                componentIdList += compId.ToString()+", ";
                            }
                            else//Add without comma (last one)
                            {
                                componentIdList += compId.ToString();
                            }
                        }
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readComponents.Replace("@0", componentIdList);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var component = new Component();
                                    component.ComponentId = reader.GetInt32(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    component.Name = reader[1] as string;
                                    component.Description = reader[2] as string;
                                    components.Add(component);
                                }
                            }
                        }
                    }
                    conn.Close();

                    //Set total review number
                    statsInfo.totalReviews = posts.Count;

                    //Get life span and issue free span information
                    List<TimeSpan> lifeSpans = new List<TimeSpan>();
                    List<TimeSpan> issueFreeSpans = new List<TimeSpan>();
                    /*If the date the product became innoperative or staerted presenting issues is null, consider it as 
                     * present date (i.e. it has worked/been fine until today)*/
                    foreach (Post post in posts)
                    {
                        if (post.Innoperative == null)
                        {
                            post.Innoperative = DateTime.Today;
                        }
                        if (post.FirstIssues == null)
                        {
                            post.FirstIssues = DateTime.Today;
                        }
                    }
                    //Get each post's life span and issue free span
                    foreach (Post post in posts)
                    {
                        lifeSpans.Add(post.Innoperative.Value.Subtract(post.Purchase));
                        issueFreeSpans.Add(post.FirstIssues.Value.Subtract(post.Purchase));
                    }
                    //Set averages, the is a function for this but I couldnt get it to work due to IQueryable so i made one
                    statsInfo.lifespan = SpanAverage(lifeSpans);
                    statsInfo.issueFree = SpanAverage(issueFreeSpans);

                    //Set Issues data
                    foreach(Component component in components)
                    {
                        IssuesInfo issuesInfo = new IssuesInfo();
                        issuesInfo.component = component;
                        double issueCount = 0;
                        double fixableCount = 0;
                        foreach (Issue issue in issues)
                        {
                            if(issue.ComponentId == component.ComponentId)
                            {
                                issueCount++;
                                if (issue.IsFixable)
                                {
                                    fixableCount++;
                                }
                            }
                        }
                        issuesInfo.percentIssues = issueCount / statsInfo.totalReviews;
                        issuesInfo.percentFixable = fixableCount / issueCount;
                        statsInfo.componentIssues.Add(issuesInfo);
                    }
                }
                return Ok(statsInfo);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        //Average TimeSpan out of a List of TimeSpans
        private TimeSpan SpanAverage(List<TimeSpan> spans)
        {
            double secondsAverage = 0;
            foreach (TimeSpan span in spans)
            {
                secondsAverage += span.TotalSeconds;
            }
            secondsAverage /= spans.Count;
            return TimeSpan.FromSeconds(secondsAverage);
        }
    }
}