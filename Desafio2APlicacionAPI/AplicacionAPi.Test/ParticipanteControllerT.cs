using Desafio2APlicacionAPI.Controllers;
using Desafio2APlicacionAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplicacionAPi.Test
{
    public class ParticipanteControllerT
    {
        [Fact]
        public async Task GetLibro_RetornaLibro_CuandoIdEsValido()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var controller = new ParticipantesController(context);
            var libro = new Participante { Nombre = "MA", Email = "a@gmail.com", EventoId = 1, };
            context.Participante.Add(libro);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetParticipanteT(libro.ParticipanteId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Participante>>(result);
            var returnValue = Assert.IsType<Participante>(actionResult.Value);
            Assert.Equal("MA", returnValue.Nombre);
        }

        [Fact]
        public async Task GetLibro_RetornaNotFound_CuandoIdNoExiste()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var controller = new ParticipantesController(context);

            // Act
            var result = await controller.GetParticipanteT(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
