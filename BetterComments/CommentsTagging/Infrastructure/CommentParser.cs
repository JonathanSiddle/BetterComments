﻿using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
   internal abstract class CommentParser : ICommentParser
   {
      protected readonly StringComparison OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase;

      protected readonly Settings Settings = Settings.Instance;

      #region ICommentParser Members

      public virtual Comment Parse(SnapshotSpan span)
      {
         var commentType = GetCommentType(span);

         if (commentType == CommentType.Normal)
            return new Comment(new List<SnapshotSpan> { span }, CommentType.Normal);

         // Color only the "Todo" keyword.
         if (Settings.HighlightTaskKeywordOnly && commentType == CommentType.Task)
         {
            var spanText = span.GetText().ToLower();
            var token = Settings.GetTokenValue(CommentType.Task);
            var start = spanText.IndexOf(token, OrdinalIgnoreCase);

            return new Comment(
                new SnapshotSpan(span.Snapshot, span.Start + start, token.Length),
                CommentType.Task);
         }

         return SpecificParse(span, commentType);
      }

      public abstract bool IsValidComment(SnapshotSpan span);

      #endregion ICommentParser Members

      protected virtual CommentType GetCommentType(SnapshotSpan span)
      {
         var commentText = SpanTextWithoutCommentStarter(span).ToLower().Trim();

         if (commentText.StartsWith(Settings.GetTokenValue(CommentType.Important), OrdinalIgnoreCase))
            return CommentType.Important;

         if (commentText.StartsWith(Settings.GetTokenValue(CommentType.Question), OrdinalIgnoreCase))
            return CommentType.Question;

         if (commentText.StartsWith(Settings.GetTokenValue(CommentType.Crossed), OrdinalIgnoreCase))
            return CommentType.Crossed;

         if (commentText.StartsWith(Settings.GetTokenValue(CommentType.Task), OrdinalIgnoreCase))
            return CommentType.Task;

         return CommentType.Normal;
      }

      protected abstract Comment SpecificParse(SnapshotSpan span, CommentType commentType);

      protected abstract string SpanTextWithoutCommentStarter(SnapshotSpan span);
   }
}