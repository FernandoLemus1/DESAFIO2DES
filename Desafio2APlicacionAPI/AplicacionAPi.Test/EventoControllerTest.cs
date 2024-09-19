using Desafio2APlicacionAPI.Controllers;
using Desafio2APlicacionAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplicacionAPi.Test
{
    public class EventoControllerTest
    {
        [Fact]
        public async Task PostEvento_AgregaEvento_CuandoDatosSonValidos()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();

            // Simula la conexión a Redis
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            // Simula que no pasa nada cuando se borra la clave
            mockDb.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                  .ReturnsAsync(true);

            // Pasa el mock de Redis al controlador
            var controller = new EventosController(context, mockRedis.Object);
            var nuevoEvento = new Evento { Nombre = "Evento Test", Fecha = DateTime.Now, Lugar = "Lugar Test" };

            // Act
            var result = await controller.PostEvento(nuevoEvento);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var eventoCreado = Assert.IsType<Evento>(actionResult.Value);
            Assert.Equal("Evento Test", eventoCreado.Nombre);
            Assert.Equal("Lugar Test", eventoCreado.Lugar);
        }



     


        [Fact]
        public async Task GetEvento_RetornaNotFound_CuandoIdNoExiste()
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

            var controller = new EventosController(context, mockRedis.Object);

            // Act
            var result = await controller.GetEventoT(999);  // ID que no existe

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);  // Verifica que se retorne NotFound (404)
        }



        [Fact]
        public async Task DeleteEvento_RetornaNotFound_CuandoIdNoExiste()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();

            // Simula la conexión a Redis
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            // Simula que no pasa nada cuando se intenta borrar una clave que no existe
            mockDb.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                  .ReturnsAsync(false);

            var controller = new EventosController(context, mockRedis.Object);

            // Act
            var result = await controller.DeleteEvento(999);  // ID que no existe

            // Assert
            Assert.IsType<NotFoundResult>(result);  // Verificar que el resultado es NotFound (404)
        }


    }
}
