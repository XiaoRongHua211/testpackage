using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Aliyun.OSS;
using HumanSDK.VO.Req;
using HumanSDK.VO.Resp;
using Newtonsoft.Json;

namespace HumanSdk.Core.Service
{
    public class HumanApiService
    {
        // 登陆接口
        public async Task<LoginResp> Login(string host, HumanSDK.VO.Req.LoginReq req)
        {
            string loginUrl = $"{host}/api/authenticate/v1/user/login";

            var response = await HttpUtil.PostJsonAsync<HumanSDK.VO.Req.LoginReq, LoginResp>(
                url: loginUrl,
                data: req);

            return response ?? throw new InvalidOperationException("服务器返回空响应");
        }

        //获取项目
        public async Task<ProjectsResp> GetProjects(string host, ProjectReq req, Dictionary<string, string> headers)
        {
            string projectUrl = $"{host}/api/asset/v1/project/get";

            var response = await HttpUtil.PostJsonAsync<ProjectReq, ProjectsResp>(
                url: projectUrl,
                data: req,
                headers: headers);

            return response ?? throw new InvalidOperationException("服务器返回空响应");
        }

        // 拉取任务接口
        public async Task<JobResp> PullJob(string host, JobReq req, Dictionary<string, string>? headers)
        {
            string pullUrl = $"{host}/api/asset/v1/human-case/pull-job";

            var response = await HttpUtil.PostJsonAsync<JobReq, JobResp>(
                url: pullUrl,
                data: req,
                headers: headers);

            return response ?? throw new InvalidOperationException("服务器返回空响应");
        }

        // 任务撤销
        public async Task<WithDrawJobResp> WithDrawJob(string host, WithDrawJobReq req, Dictionary<string, string>? headers)
        {
            string pullUrl = $"{host}/api/asset/v1/human-case/withdraw-job";

            var response = await HttpUtil.PostJsonAsync<WithDrawJobReq, WithDrawJobResp>(
                url: pullUrl,
                data: req,
                headers: headers);

            return response ?? throw new InvalidOperationException("服务器返回空响应");
        }

        // 完成任务接口
        public async Task<CompleteJobResp> CompleteJob(string host, CompleteJobReq req, Dictionary<string, string>? headers)
        {
            string region = req.region;
            string bucketName = "lw-human-case";
            if (region.Contains("test"))
            {
                bucketName = "lw-human-case-test";
            }

            List<HumanFileCreate> originalFiles = new List<HumanFileCreate>();
            if (req.humanFileCreates != null && req.humanFileCreates.Count > 0)
            {
                foreach (var fileCreate in req.humanFileCreates)
                {
                    fileCreate.bucketType = 2;
                    fileCreate.bucketName = bucketName;
                    if (fileCreate.fileType == "human_case_video")
                    {
                        HumanFileCreate originalFile = new HumanFileCreate();
                        originalFile.bucketName = fileCreate.bucketName;
                        originalFile.bucketType = fileCreate.bucketType;
                        originalFile.fileName = fileCreate.fileName;
                        originalFile.filePath = fileCreate.filePath;
                        originalFile.fileSize = fileCreate.fileSize;
                        originalFile.fileType = "human_case_video_original";
                        originalFiles.Add(originalFile);
                    }
                }
            }

            if (originalFiles.Count > 0)
            {
                req.humanFileCreates.AddRange(originalFiles);
            }

            string completeJobUrl = $"{host}/api/asset/v1/human-case/complete-job";
            var response = await HttpUtil.PostJsonAsync<CompleteJobReq, CompleteJobResp>(
                url: completeJobUrl,
                data: req,
                headers: headers);

            return response ?? throw new InvalidOperationException("服务器返回空响应");
        }

        public async Task<CompleteProduceingJobResp> CompleteProduceingJob(string host, CompleteJobReq req, Dictionary<string, string>? headers)
        {
            string region = req.region;
            string bucketName = "lw-human-case";
            if (region.Contains("test"))
            {
                bucketName = "lw-human-case-test";
            }

            List<HumanFileCreate> originalFiles = new List<HumanFileCreate>();
            if (req.humanFileCreates != null && req.humanFileCreates.Count > 0)
            {
                foreach (var fileCreate in req.humanFileCreates)
                {
                    fileCreate.bucketType = 2;
                    fileCreate.bucketName = bucketName;
                    if (fileCreate.fileType == "human_case_video")
                    {
                        HumanFileCreate originalFile = new HumanFileCreate();
                        originalFile.bucketName = fileCreate.bucketName;
                        originalFile.bucketType = fileCreate.bucketType;
                        originalFile.fileName = fileCreate.fileName;
                        originalFile.filePath = fileCreate.filePath;
                        originalFile.fileSize = fileCreate.fileSize;
                        originalFile.fileType = "human_case_video_original";
                        originalFiles.Add(originalFile);
                    }
                }
            }

            if (originalFiles.Count > 0)
            {
                req.humanFileCreates.AddRange(originalFiles);
            }

            string completeJobUrl = $"{host}/api/asset/v1/human-case/complete-job";
            var response = await HttpUtil.PostJsonAsync<CompleteJobReq, CompleteProduceingJobResp>(
                url: completeJobUrl,
                data: req,
                headers: headers);

            return response ?? throw new InvalidOperationException("服务器返回空响应");
        }

        // 获取任务
        public async Task<ListHumanTaskResp> ListHumanTask(string host, ListHumanTaskReq req, Dictionary<string, string>? headers)
        {
            string listTaskUrl = $"{host}/api/asset/v1/human-case/list-task";

            var response = await HttpUtil.PostJsonAsync<ListHumanTaskReq, ListHumanTaskResp>(
                url: listTaskUrl,
                data: req,
                headers: headers);

            return response ?? throw new InvalidOperationException("服务器返回空响应");
        }

        public async Task<UserInfoResp> GetCurrentUserInfo(string host, UserInfoReq req, Dictionary<string, string>? headers)
        {
            string listTaskUrl = $"{host}/api/authenticate/v1/user/current_user_info";

            var response = await HttpUtil.PostJsonAsync<UserInfoReq, UserInfoResp>(
                url: listTaskUrl,
                data: req,
                headers: headers);

            return response ?? throw new InvalidOperationException("服务器返回空响应");
        }
    }
}