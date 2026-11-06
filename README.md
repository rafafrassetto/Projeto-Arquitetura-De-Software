# Sistema de Estacionamento Rotativo

Sistema distribuído baseado em microsserviços para gestão e controle de pátios de estacionamento rotativo, gerenciamento de vagas, fluxo de veículos e liquidação de estadias.

---

## Visão Geral do Sistema

1. **Descrição Geral:**
   * Software para controle de estacionamento rotativo, permitindo o cadastro e acompanhamento de **vagas**, **veículos** e **cobranças de estadia**.

2. **Arquitetura em Microsserviços:**
   * O sistema é composto por 4 microsserviços independentes:
     * `vagas-service`: Gerencia a disponibilidade, tipos e alocação de vagas no pátio.
     * `veiculos-service`: Cadastro e histórico de veículos (placa, modelo, condutor).
     * `operadores-service`: Gestão de operadores de caixa, manobristas e administradores.
     * `cobranca-service`: Controle de tarifas, emissão de tickets, cálculo de permanência e pagamentos.

---

## Fluxos de Consulta e Transação

### 3. Primeira Busca de Dados (Ocupação de Vagas)
* **Objetivo:** Consultar o veículo ocupante a partir de uma vaga.
* **Comportamento:** Ao buscar os detalhes de uma vaga específica (`vagas-service`), o serviço realiza uma comunicação para exibir os dados do veículo atualmente alocado nela (`veiculos-service`).

### 4. Segunda Busca de Dados (Validação de Cobrança)
* **Objetivo:** Validar o status da estadia antes do fechamento.
* **Comportamento:** Ao registrar a cobrança/fechamento (`cobranca-service`), o sistema busca o veículo (`veiculos-service`) e valida se o registro de entrada ainda está ativo no pátio.

### 5. Transação de Alteração de Dados (Liquidação e Liberação)
* **Objetivo:** Atualização síncrona/reativa do status da vaga.
* **Comportamento:** A transação de alteração ocorre no microsserviço de cobrança (`cobranca-service`). Ao confirmar o pagamento da estadia, o status da respectiva vaga é automaticamente atualizado para **"Liberada"** no cadastro de vagas (`vagas-service`).

