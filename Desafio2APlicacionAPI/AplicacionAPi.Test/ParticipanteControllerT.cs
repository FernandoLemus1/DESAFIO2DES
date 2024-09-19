using Desafio2APlicacionAPI.Controllers;
using Desafio2APlicacionAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using StackExchange.Redis;
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
        public async Task GetParticipante_RetornaParticipante_CuandoIdEsValido()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();

            // Simula la conexión a Redis
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            // Simula que no hay un valor en caché
            mockDb.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            var controller = new ParticipantesController(context, mockRedis.Object);
            var participante = new Participante { Nombre = "MA", Email = "a@gmail.com", EventoId = 1 };
            context.Participante.Add(participante);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetParticipante(participante.ParticipanteId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Participante>>(result);
            var returnValue = Assert.IsType<Participante>(actionResult.Value);
            Assert.Equal("MA", returnValue.Nombre);
        }

        [Fact]
        public async Task GetParticipante_RetornaNotFound_CuandoIdNoExiste()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();

            // Simula la conexión a Redis
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            // Simula que no hay un valor en caché
            mockDb.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            var controller = new ParticipantesController(context, mockRedis.Object);

            // Act
            var result = await controller.GetParticipante(999);  // ID que no existe

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task PostParticipante_IncrementaConteo_CuandoSeAgregaNuevoParticipante()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();

            // Simula la conexión a Redis
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            var controller = new ParticipantesController(context, mockRedis.Object);
            var participanteInicial = new Participante { Nombre = "AAA", Email = "a@gmail.com", EventoId = 1 };

            await controller.PostParticipante(participanteInicial);

            var nuevoParticipante = new Participante { Nombre = "AAA2", Email = "a2@gmail.com", EventoId = 1 };

            // Act
            await controller.PostParticipanteT(nuevoParticipante);
            var participantes = await context.Participante.ToListAsync();

            // Assert
            Assert.Equal(2, participantes.Count);
        }

        [Fact]
        public async Task DeleteParticipante_EliminaParticipante_CuandoIdEsValido()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();

            // Simula la conexión a Redis
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            var controller = new ParticipantesController(context, mockRedis.Object);
            var participante = new Participante { Nombre = "AEliminar", Email = "aeliminar@gmail.com", EventoId = 1 };
            context.Participante.Add(participante);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteParticipante(participante.ParticipanteId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var participanteEnDb = await context.Participante.FindAsync(participante.ParticipanteId);
            Assert.Null(participanteEnDb);
        }



    }
}
