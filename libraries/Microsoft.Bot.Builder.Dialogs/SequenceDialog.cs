﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Dialog optimized for prompting a user with a series of questions. Sequences accept a stack of
    /// functions which will be executed in sequence.Each sequence step can ask a question of the user
    /// and the users response will be passed as an argument to the next sequence step.
    /// </summary>
    public class SequenceDialog : ComponentDialog
    {
        private const string PersistedOptions = "options";
        private const string StepIndex = "stepIndex";
        private const string PersistedValues = "values";

        public SequenceDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDialog"/> class.
        /// </summary>
        /// <param name="dialogId">The dialog id.</param>
        /// <param name="dialogs">Optional steps to be defined by caller.</param>
        public SequenceDialog(string dialogId)
            : base(dialogId)
        {
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Initialize sequence state
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = options;
            state[PersistedValues] = new Dictionary<string, object>();

            // Run first step
            return await RunStepAsync(dc, 0, DialogReason.BeginCalled, null, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Don't do anything for non-message activities.
            if (dc.Context.Activity.Type != ActivityTypes.Message)
            {
                return Dialog.EndOfTurn;
            }

            // Run next step with the message text as the result.
            return await ResumeDialogAsync(dc, DialogReason.ContinueCalled, dc.Context.Activity.Text, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Increment step index and run step
            var state = dc.ActiveDialog.State;

            // For issue https://github.com/Microsoft/botbuilder-dotnet/issues/871
            // See the linked issue for details. This issue was happening when using the CosmosDB
            // data store for state. The stepIndex which was an object being cast to an Int64
            // after deserialization was throwing an exception for not being Int32 datatype.
            // This change ensures the correct datatype conversion has been done.
            var index = Convert.ToInt32(state[StepIndex]);
            return await RunStepAsync(dc, index + 1, reason, result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> RunStepAsync(DialogContext dc, int index, DialogReason reason, object result, CancellationToken cancellationToken)
        {
            //if (dc == null)
            //{
            //    throw new ArgumentNullException(nameof(dc));
            //}

            //if (index < Steps.Count)
            //{
            //    // Update persisted step index
            //    var state = dc.ActiveDialog.State;
            //    state[StepIndex] = index;

            //    // Create step context
            //    var options = state[PersistedOptions];
            //    var values = (IDictionary<string, object>)state[PersistedValues];
            //    var stepContext = new SequenceStepContext(this, dc, options, values, index, reason, result);

            //    // Execute step
            //    return await OnStepAsync(stepContext, cancellationToken).ConfigureAwait(false);
            //}
            //else
            //{
            //    // End of sequence so just return any result to parent
            //    return await dc.EndDialogAsync(result).ConfigureAwait(false);
            //}
            return await dc.EndDialogAsync(reason).ConfigureAwait(false);
        }
    }
}