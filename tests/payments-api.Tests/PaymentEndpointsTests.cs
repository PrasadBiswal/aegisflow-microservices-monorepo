using System.Net;
using System.Net.Http.Json;
using AegisFlow.Payments.Api.Contracts;

namespace AegisFlow.Payments.Api.Tests;

public sealed class PaymentEndpointsTests(PaymentsApiFactory factory) : IClassFixture<PaymentsApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Payment_can_be_captured_then_refunded()
    {
        var payment = await CreatePaymentAsync(Guid.NewGuid());

        var capture = await _client.PostAsync($"/api/payments/{payment.Id}/capture", null);
        var refund = await _client.PostAsync($"/api/payments/{payment.Id}/refund", null);

        Assert.Equal(HttpStatusCode.OK, capture.StatusCode);
        Assert.Equal(HttpStatusCode.OK, refund.StatusCode);
    }

    [Fact]
    public async Task Pending_payment_cannot_be_refunded()
    {
        var payment = await CreatePaymentAsync(Guid.NewGuid());

        var refund = await _client.PostAsync($"/api/payments/{payment.Id}/refund", null);

        Assert.Equal(HttpStatusCode.Conflict, refund.StatusCode);
    }

    [Fact]
    public async Task Second_open_payment_for_the_same_order_is_rejected()
    {
        var orderId = Guid.NewGuid();
        await CreatePaymentAsync(orderId);

        var second = await _client.PostAsJsonAsync("/api/payments", new CreatePaymentRequest(orderId, 10m, "USD", "Card"));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private async Task<PaymentResponse> CreatePaymentAsync(Guid orderId)
    {
        var response = await _client.PostAsJsonAsync("/api/payments", new CreatePaymentRequest(orderId, 25m, "usd", "Card"));
        response.EnsureSuccessStatusCode();
        var payment = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        Assert.NotNull(payment);
        return payment;
    }
}
