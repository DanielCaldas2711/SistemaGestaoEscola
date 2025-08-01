﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SistemaGestaoEscola.Web.Data;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.Helpers;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using System.Linq;

namespace SistemaGestaoEscola.Web.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassApiController : ControllerBase
    {        
        private readonly IClassRepository _classRepository;
        private readonly IClassStudentsRepository _classStudentsRepository;

        public ClassApiController(IClassRepository classRepository,
            IClassStudentsRepository classStudentsRepository)
        {            
            _classRepository = classRepository;
            _classStudentsRepository = classStudentsRepository;
        }

        /// <summary>
        /// Retorna todos os alunos de uma turma pelo ID da turma.
        /// </summary>
        /// <param name="classId">ID da turma</param>
        /// <returns>Lista de alunos</returns>
        [HttpGet("{classId}/students")]
        public async Task<IActionResult> GetStudents(int classId)
        {
            var classEntity = await _classRepository.GetByIdAsync(classId);

            if (classEntity == null)
            {
                return NotFound(new { message = "Turma não encontrada." });
            }

            var studentEntity = await _classStudentsRepository.GetAllStudentsFromClass(classEntity.Id);

            var students = studentEntity
                .Select(cs => new
                {
                    cs.Id,
                    cs.FullName,
                    cs.Email
                });

            return Ok(students);
        }
    }
}
