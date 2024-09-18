using Desafio2APlicacionAPI.Controllers;
using Desafio2APlicacionAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [Fact]
        public async Task PostParticipante_NoAgregaParticipante_CuandoNombreEsNulo()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var controller = new ParticipantesController(context);
            var nuevoParticipante = new Participante { Nombre = null, Email = "a@gmail.com", EventoId = 1 };

            // Act
            var result = await controller.PostParticipanteT(nuevoParticipante);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result); // Valida que se retorna un BadRequest
        }

        [Fact]
        public async Task PostParticipante_IncrementaConteo_CuandoSeAgregaNuevoParticipante()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var controller = new ParticipantesController(context);
            var participanteInicial = new Participante { Nombre = "AAA", Email = "a@gmail.com", EventoId = 1 };

            await controller.PostParticipanteT(participanteInicial);

            var nuevoParticipante = new Participante { Nombre = "AAA2", Email = "a2@gmail.com", EventoId = 1 };

            // Act
            await controller.PostParticipanteT(nuevoParticipante);
            var participantes = await context.Participante.ToListAsync();

            // Assert
            Assert.Equal(2, participantes.Count); // Asegúrate que el conteo sea correcto
        }

      /*  [Fact]
        public async Task PutParticipante_ActualizaParticipante_CuandoDatosSonValidos()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var controller = new ParticipantesController(context);
            var participante = new Participante { Nombre = "Original", Email = "original@gmail.com", EventoId = 1 };
            context.Participante.Add(participante);
            await context.SaveChangesAsync();

            var participanteActualizado = new Participante { ParticipanteId = participante.ParticipanteId, Nombre = "Actualizado", Email = "actualizado@gmail.com", EventoId = 1 };

            // Act
            var result = await controller.PutParticipanteT(participante.ParticipanteId, participanteActualizado);

            // Assert
            Assert.IsType<NoContentResult>(result);  // Verifica que el resultado sea NoContentResult (204)

            var participanteEnDb = await context.Participante.FindAsync(participante.ParticipanteId);
            Assert.Equal("Actualizado", participanteEnDb.Nombre);
            Assert.Equal("actualizado@gmail.com", participanteEnDb.Email);
        }*/

        [Fact]
        public async Task DeleteParticipante_EliminaParticipante_CuandoIdEsValido()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var controller = new ParticipantesController(context);
            var participante = new Participante { Nombre = "AEliminar", Email = "aeliminar@gmail.com", EventoId = 1 };
            context.Participante.Add(participante);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteParticipanteT(participante.ParticipanteId);

            // Assert
            Assert.IsType<NoContentResult>(result);  // El método Delete debe retornar un NoContentResult
            var participanteEnDb = await context.Participante.FindAsync(participante.ParticipanteId);
            Assert.Null(participanteEnDb);  // Verificamos que el participante ya no existe
        }


    }
}
